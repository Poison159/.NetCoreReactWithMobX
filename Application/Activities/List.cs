using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistance;

namespace Application.Activities
{
    public class List
    {
        public class Query:IRequest<Result<List<ActivityDto>>> {};

        public class Handler : IRequestHandler<Query, Result<List<ActivityDto>>>
        {
            public DataContext _context { get; }
            public IMapper _mapper { get; }

            public Handler(DataContext context,IMapper mapper)
            {
                this._mapper = mapper;
                this._context = context;
            }

            public async Task<Result<List<ActivityDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var activites = await _context.Activities
                .ProjectTo<ActivityDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

                return Result<List<ActivityDto>>.Success(activites);
            }
        }
    }
}