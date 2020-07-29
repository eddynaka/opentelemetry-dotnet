// <copyright file="CustomPropagator1.cs" company="OpenTelemetry Authors">
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
using System.Linq;
using OpenTelemetry.Context.Propagation;

namespace OpenTelemetry.Tests.Implementation.Trace.Propagation
{
    public class CustomPropagator1 : ITextFormat
    {
        public ISet<string> Fields => throw new NotImplementedException();

        public ActivityContext Extract<T>(ActivityContext activityContext, T carrier, Func<T, string, IEnumerable<string>> getter)
        {
            return activityContext;
        }

        public void Inject<T>(ActivityContext activityContext, T carrier, Action<T, string, string> setter)
        {
            setter(carrier, "custom-tag", "custom-propagator-1");
            return;
        }

        public bool IsInjected<T>(T carrier, Func<T, string, IEnumerable<string>> getter)
        {
            return getter(carrier, "custom-tag").Any();
        }
    }
}
