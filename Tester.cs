using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Xml.Linq;
using Resto.Front.Api.Data.Cheques;
using Resto.Front.Api.Data.View;
using Resto.Front.Api.UI;
using Resto.Front.Api.Extensions;
using Resto.Front.Api;

namespace TNG_plugin
{

    internal sealed class Tester : IDisposable
    {
        private readonly CompositeDisposable subscriptions;
        public Tester()
        {

        }
            public void Dispose()
        {
            subscriptions.Dispose();
        }
    }
}
