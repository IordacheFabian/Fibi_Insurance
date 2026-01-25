using System;
using Application.Core;
using AutoMapper;


namespace Application.Tests.TestHelpers;

public class TestMapper
{
    private static IMapper? _mapper ;
    public static IMapper Instance
    {
        get
        {
            if (_mapper != null) return _mapper;


            var config = new MapperConfigurationExpression();
            config.AddProfile<MappingProfiles>();


            var mapperConfig = new MapperConfiguration(config);
            _mapper = mapperConfig.CreateMapper();


            return _mapper;
        }
    }
}
