// <copyright file="CompositePropagatorTest.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTelemetry.Context.Propagation;
using Xunit;

namespace OpenTelemetry.Tests.Implementation.Trace.Propagation
{
    public class CompositePropagatorTest
    {
        private const string TraceParent = "traceparent";

        private static readonly string[] Empty = new string[0];
        private static readonly Func<IDictionary<string, string>, string, IEnumerable<string>> Getter = (headers, name) =>
        {
            if (headers.TryGetValue(name, out var value))
            {
                return new[] { value };
            }

            return Empty;
        };

        private static readonly Action<IDictionary<string, string>, string, string> Setter = (carrier, name, value) =>
        {
            carrier[name] = value;
        };

        [Fact]
        public void CompositePropagator_EmptyTextFormatList()
        {
            Dictionary<string, string> expected = new Dictionary<string, string>();
            Dictionary<string, string> carrier = new Dictionary<string, string>();
            var compositePropagator = new CompositePropagator();
            var activityContext = compositePropagator.Extract(default, carrier, Getter);
            Assert.Equal(default, activityContext);

            var traceId = ActivityTraceId.CreateRandom();
            var spanId = ActivitySpanId.CreateRandom();
            activityContext = new ActivityContext(traceId, spanId, ActivityTraceFlags.Recorded, traceState: null);
            compositePropagator.Inject(activityContext, carrier, Setter);
            Assert.Equal(expected, carrier);
        }

        [Fact]
        public void CompositePropagator_WithTraceContext()
        {
            var traceId = ActivityTraceId.CreateRandom();
            var spanId = ActivitySpanId.CreateRandom();
            var expectedHeaders = new Dictionary<string, string>
            {
                { TraceParent, $"00-{traceId}-{spanId}-01" },
            };

            var compositePropagator = new CompositePropagator(new List<ITextFormat>
            {
                new TraceContextFormat(),
            });

            var activityContext = new ActivityContext(traceId, spanId, ActivityTraceFlags.Recorded, traceState: null);
            var carrier = new Dictionary<string, string>();
            compositePropagator.Inject(activityContext, carrier, Setter);

            Assert.Equal(expectedHeaders, carrier);

            var ctx = compositePropagator.Extract(activityContext, expectedHeaders, Getter);
            Assert.Equal(activityContext.TraceId, ctx.TraceId);
            Assert.Equal(activityContext.SpanId, ctx.SpanId);
        }

        [Fact]
        public void CompositePropagator_WithTraceContextAndB3Format()
        {
            var traceId = ActivityTraceId.CreateRandom();
            var spanId = ActivitySpanId.CreateRandom();
            var expectedHeaders = new Dictionary<string, string>
            {
                { TraceParent, $"00-{traceId}-{spanId}-01" },
                { B3Format.XB3TraceId, traceId.ToString() },
                { B3Format.XB3SpanId, spanId.ToString() },
                { B3Format.XB3Sampled, "1" },
            };

            var compositePropagator = new CompositePropagator(new List<ITextFormat>
            {
                new TraceContextFormat(),
                new B3Format(),
            });

            var activityContext = new ActivityContext(traceId, spanId, ActivityTraceFlags.Recorded, traceState: null);
            var carrier = new Dictionary<string, string>();
            compositePropagator.Inject(activityContext, carrier, Setter);

            Assert.Equal(expectedHeaders, carrier);

            var ctx = compositePropagator.Extract(activityContext, expectedHeaders, Getter);
            Assert.Equal(activityContext.TraceId, ctx.TraceId);
            Assert.Equal(activityContext.SpanId, ctx.SpanId);
            Assert.True(ctx.IsValid());
        }

        [Fact]
        public void CompositePropagator_ThrowNotImplementedException()
        {
            var compositePropagator = new CompositePropagator();

            Assert.Throws<NotImplementedException>(() => compositePropagator.Fields);
            Assert.Throws<NotImplementedException>(() => compositePropagator.IsInjected(null, Getter));
        }
    }
}
