using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;

namespace GrpcClient
{
    class Program
    {
        private GrpcChannel _channel;
        private Parts.PartsClient _client;
        private List<SubPart> _subPartsList = new();
        private Part _currentPart;

        static async Task Main(string[] args)
        {
            var program = new Program();
            Console.WriteLine("Lista de comandos: ");
            Console.WriteLine("bind [host:port] - cria uma conexão com o servidor remoto");
            Console.WriteLine("showbind - mostra a conexão atual");
            Console.WriteLine("listp - lista as peças do servidor atual");
            Console.WriteLine("getp [cod] - obtem a peça através do código");
            Console.WriteLine("showp - mostra os detalhes da peça atual");
            Console.WriteLine("addp - adiciona uma peça ao servidor atual");
            Console.WriteLine("listsubp - exibe a lista de sub pecas atual");
            Console.WriteLine("addsubp - adiciona a peça atual como uma sub peça a lista");
            Console.WriteLine("clearsubp - esvazia a lista de sub peças atual");
            Console.WriteLine("quit - encerra a execução");
            Console.WriteLine("");
            Console.WriteLine("");

            var command = "";
            string[] commands;

            while (command != "quit")
            {
                command = Console.ReadLine();
                switch (command)
                {
                    case string s when s.StartsWith("bind"):
                        commands = command.Split(" ");
                        if (commands.Length <= 1)
                        {
                            Console.WriteLine("Comando inválido!");
                            break;
                        }
                        var serverAddress = commands[1];
                        try
                        {
                            program.Connect(serverAddress);
                            Console.WriteLine("");
                            Console.WriteLine("Conectado com sucesso!");
                        }
                        catch
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Erro ao se conectar com o servidor!");
                            program.DisposeConnections();
                        }
                        break;
                    case string s when s.StartsWith("getp"):
                        commands = command.Split(" ");
                        if(commands.Length <= 1)
                        {
                            Console.WriteLine("Comando inválido!");
                            break;
                        }
                        var partCode = commands[1];
                        var part = await program.GetPart(int.Parse(partCode));
                        if (part is not null)
                        {
                            program._currentPart = part;
                            Console.WriteLine("Peça obtida com sucesso!");
                        }
                        else
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Peça não encontrada!");
                        }
                        break;
                    case "addp":
                        Console.WriteLine("");
                        var result = await program.AddPart();
                        if (result)
                        {
                            Console.WriteLine("Peça adicionada com sucesso!");
                        }
                        else
                        {
                            Console.WriteLine("Erro ao adicionar peça!");
                        }
                        break;
                    case "listp":
                        Console.WriteLine("");
                        await program.ListParts();
                        break;
                    case "showp":
                        Console.WriteLine("");
                        program.ShowPart();
                        break;
                    case "showbind":
                        Console.WriteLine("");
                        program.ShowConnection();
                        break;
                    case "listsubp":
                        Console.WriteLine("");
                        program.ListSubPart();
                        break;
                    case "addsubp":
                        Console.WriteLine("");
                        program.AddSubPart();
                        break;
                    case "clearsubp":
                        Console.WriteLine("");
                        program.ClearSubPartsList();
                        break;
                    case "quit":
                        break;
                    default:
                        Console.WriteLine("Comando inválido! Tente Novamente");
                        break;
                }
                Console.WriteLine("");
            }
            Console.WriteLine("");
            Console.WriteLine("Pessione qualque tecla para fechar o programa");
            Console.ReadKey();
        }

        private void Connect(string server)
        {
            DisposeConnections();
            _channel = GrpcChannel.ForAddress($"https://{server}");
            _client = new Parts.PartsClient(_channel);

            //test connection
            _client.ListParts(new ListPartsRequest());
        }

        private async Task ListParts()
        {
            try
            {
                if (_client is not null)
                {
                    var listPartsResponse = await _client.ListPartsAsync(new ListPartsRequest());
                    if (!listPartsResponse.Parts.Any())
                    {
                        Console.WriteLine("Não existe nenhuma peça no servidor!");
                    }
                    foreach (Part part in listPartsResponse.Parts)
                    {
                        Console.WriteLine($"Código: {part.Code}");
                        Console.WriteLine($"Descrição: {part.Description}");
                        Console.WriteLine($"Nome: {part.Name}");
                        Console.WriteLine($"Sub-peças:");
                        foreach (SubPart subPart in part.SubParts)
                        {
                            Console.WriteLine($"\tCódigo: {subPart.Code}");
                            Console.WriteLine($"\tQuantidade: {subPart.Quantity}");
                            Console.WriteLine("");
                        }
                        Console.WriteLine("");
                    }
                }
                else
                {
                    Console.WriteLine("Não existe nenhuma conexão ativa com o servidor");
                }
            }
            catch
            {
                Console.WriteLine("Erro ao obter a lista de peças!");
            }

        }

        private void ShowPart()
        {
            try
            {
                if (_currentPart is not null)
                {
                    Console.WriteLine($"Código: {_currentPart.Code}");
                    Console.WriteLine($"Descrição: {_currentPart.Description}");
                    Console.WriteLine($"Nome: {_currentPart.Name}");
                    Console.WriteLine($"Sub-peças:");
                    foreach (SubPart subPart in _currentPart.SubParts)
                    {
                        Console.WriteLine($"\tCódigo: {subPart.Code}");
                        Console.WriteLine($"\tQuantidade: {subPart.Quantity}");
                        Console.WriteLine("");
                    }
                }
                else
                {
                    Console.WriteLine("Não existe nenhuma peça atual");
                }
            }
            catch
            {
                Console.WriteLine("Erro ao exibir a peça atual");
            }
        }

        private void ShowConnection()
        {
            try
            {
                if (_channel is not null)
                {
                    Console.WriteLine($"Servidor conectado: {_channel.Target}");

                }
                else
                {
                    Console.WriteLine("Não existe nenhuma conexão no momento");
                }
            }
            catch
            {
                Console.WriteLine("Erro ao exibir a conexão atual");
            }
        }

        private async Task<Part> GetPart(int partCode)
        {
            try
            {
                if (_client is not null)
                {
                    var getPartRequest = new GetPartRequest();
                    getPartRequest.Code = partCode;
                    var partResponse = await _client.GetPartAsync(getPartRequest);
                    return partResponse.Part;
                }
                else
                {
                    Console.WriteLine("Não existe nenhuma conexão ativa com o servidor");
                }
            }
            catch
            {
                Console.WriteLine("Erro ao obter peça!");
            }
            return null;
        }
        
        private async Task<bool> AddPart()
        {
            try
            {
                if (_client is not null)
                {
                    var part = new Part();
                    Console.WriteLine("Digite o código da peça:");
                    part.Code = int.Parse(Console.ReadLine());
                    Console.WriteLine("Digite o nome da peça:");
                    part.Name = Console.ReadLine();
                    Console.WriteLine("Digite a descrição da peça:");
                    part.Description = Console.ReadLine();
                    part.SubParts.AddRange(_subPartsList);
                    var addPartRequest = new AddPartRequest
                    {
                        Part = part
                    };
                    var addPartResponse = await _client.AddPartAsync(addPartRequest);
                    if(addPartResponse.Result == true)
                    {
                        _subPartsList.Clear();
                    }
                    return addPartResponse.Result;
                }
                else
                {
                    Console.WriteLine("Não existe nenhuma conexão ativa com o servidor");
                }
            }
            catch
            {
                Console.WriteLine("Erro ao adicionar a peça!");
            }
            return false;
        }
        
        private void ListSubPart()
        {
            try
            {
                if (_client is not null)
                {
                    if (!_subPartsList.Any())
                    {
                        Console.WriteLine("Não existe nenhuma sub-peça na lista!");
                    }
                    foreach (SubPart subPart in _subPartsList)
                    {
                        Console.WriteLine($"Código: {subPart.Code}");
                        Console.WriteLine($"Quantidade: {subPart.Quantity}");
                        Console.WriteLine("");
                    }
                    Console.WriteLine("");
                }
                else
                {
                    Console.WriteLine("Não existe nenhuma conexão ativa com o servidor");
                }
            }
            catch
            {
                Console.WriteLine("Erro ao exibir a lista de sub-peças!");
            }
        }

        private void AddSubPart()
        {
            try
            {
                if (_client is not null)
                {
                    if (_currentPart is not null)
                    {
                        if(!_subPartsList.Any(p => p.Code == _currentPart.Code))
                        {
                            SubPart subPart = new();
                            subPart.Code = _currentPart.Code;
                            Console.WriteLine("Digite a quantidade da sub-peça:");
                            subPart.Quantity = int.Parse(Console.ReadLine());
                            _subPartsList.Add(subPart);
                            Console.WriteLine("Sub-peça adicionada com sucesso!");
                        }
                        else
                        {
                            Console.WriteLine("A peça atual já está na lista de sub peças!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Não existe nenhuma peça selecionada!");
                    }
                }
                else
                {
                    Console.WriteLine("Não existe nenhuma conexão ativa com o servidor");
                }
            }
            catch
            {
                Console.WriteLine("Erro ao adicionar a sub-peça!");
            }
        }

        private void ClearSubPartsList() 
        {
            try
            {
                if (_client is not null)
                {
                    _subPartsList.Clear();
                    Console.WriteLine("Lista de sub-peças evaziada com sucesso!");
                }
                else 
                {
                    Console.WriteLine("Não existe nenhuma conexão ativa com o servidor");
                }
            }
            catch
            {
                Console.WriteLine("Erro ao esvaziar a lista de sub-peças corrente!");
            }
        }

        private void DisposeConnections()
        {
            if (_channel is not null)
            {
                _channel.Dispose();
            }
            _client = null;
        }
    }
}
