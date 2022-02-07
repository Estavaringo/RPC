using Grpc.Core;
using GrpcService.Repository;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace GrpcService
{
    public class PartsService : Parts.PartsBase
    {
        private readonly ILogger<PartsService> _logger;
        private readonly PartRepository _partRepository;

        public PartsService(ILogger<PartsService> logger, PartRepository partRepository)
        {
            _logger = logger;
            _partRepository = partRepository;
        }

        public override async Task<ListPartsResponse> ListParts(ListPartsRequest request, ServerCallContext context)
        {
            var parts = new List<Part>();

            _partRepository.Parts.ForEach(part =>
            {
                var partResponse = new Part() { Code = part.Code, Name = part.Name, Description = part.Description };
                partResponse.SubParts.AddRange(part.SubParts);
                parts.Add(partResponse);
            });

            var listPartsResponse = new ListPartsResponse();
            listPartsResponse.Parts.AddRange(parts);
            return await Task.FromResult(listPartsResponse);
        }

        public override async Task<GetPartResponse> GetPart(GetPartRequest request, ServerCallContext context)
        {
            var part = _partRepository.Parts.Find(p => p.Code == request.Code);

            var getPartResponse = new GetPartResponse
            {
                Part = part
            };
            return await Task.FromResult(getPartResponse);
        }

        public override async Task<AddPartResponse> AddPart(AddPartRequest request, ServerCallContext context)
        {
            var addPartResponse = new AddPartResponse
            {
                Result = false
            };

            _partRepository.Parts.Add(request.Part);
            addPartResponse.Result = true;
            return await Task.FromResult(addPartResponse);
        }
    }
}
