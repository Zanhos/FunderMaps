﻿using AutoMapper;
using FunderMaps.Core.Entities;
using FunderMaps.WebApi.DataTransferObjects;

namespace FunderMaps.WebApi
{
    /// <summary>
    ///     Mapper profile for proper DTO mapping.
    /// </summary>
    public class MapperProfile : Profile
    {
        /// <summary>
        ///     Setup mapping profiles.
        /// </summary>
        public MapperProfile()
        {
            CreateMap<Organization, ContractorDto>();
            CreateMap<Inquiry, InquiryDto>().ReverseMap();
            CreateMap<InquiryFull, InquiryDto>()
                .ForMember(dest => dest.AuditStatus, o => o.MapFrom(src => src.State.AuditStatus))
                .ForMember(dest => dest.Reviewer, o => o.MapFrom(src => src.Attribution.Reviewer))
                .ForMember(dest => dest.Creator, o => o.MapFrom(src => src.Attribution.Creator))
                .ForMember(dest => dest.Owner, o => o.MapFrom(src => src.Attribution.Owner))
                .ForMember(dest => dest.Contractor, o => o.MapFrom(src => src.Attribution.Contractor))
                .ForMember(dest => dest.AccessPolicy, o => o.MapFrom(src => src.Access.AccessPolicy))
                .ForMember(dest => dest.CreateDate, o => o.MapFrom(src => src.Record.CreateDate))
                .ForMember(dest => dest.UpdateDate, o => o.MapFrom(src => src.Record.UpdateDate))
                .ReverseMap();
            CreateMap<InquirySample, InquirySampleDto>().ReverseMap();
            CreateMap<OrganizationProposal, OrganizationProposalDto>().ReverseMap();
            CreateMap<Project, ProjectDto>().ReverseMap();
            CreateMap<Recovery, RecoveryDto>().ReverseMap();
            CreateMap<RecoverySample, RecoverySampleDto>().ReverseMap();
            CreateMap<User, ReviewerDto>();
            CreateMap<Bundle, BundleDto>();
        }
    }
}
