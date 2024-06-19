using AutoMapper;
using OrientaTFG.Shared.Infrastructure.Enums;
using OrientaTFG.Shared.Infrastructure.Model;
using OrientaTFG.TFG.Core.DTOs;
using TFGModel = OrientaTFG.Shared.Infrastructure.Model.TFG;

namespace OrientaTFG.TFG.Core.Utils.AutoMapper;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<TFGAssignmentDTO, TFGModel>();
        CreateMap<CreateMainTaskDTO, MainTask>()
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => (int)MainTaskStatusEnum.Pendiente));
    }
}
