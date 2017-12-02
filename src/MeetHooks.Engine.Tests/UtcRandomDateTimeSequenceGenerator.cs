using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;

namespace MeetHooks.Engine.Tests
{
    public class UtcRandomDateTimeSequenceGenerator : ISpecimenBuilder
    {
        private readonly ISpecimenBuilder _innerRandomDateTimeSequenceGenerator;

        public UtcRandomDateTimeSequenceGenerator()
        {
            _innerRandomDateTimeSequenceGenerator =
                new RandomDateTimeSequenceGenerator();
        }

        public object Create(object request, ISpecimenContext context)
        {
            var result =
                _innerRandomDateTimeSequenceGenerator.Create(request, context);

            if (result is NoSpecimen)
                return result;

            return ((DateTime)result).ToUniversalTime();
        }
    }
}
