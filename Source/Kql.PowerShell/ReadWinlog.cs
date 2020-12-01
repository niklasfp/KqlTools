using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Management.Automation;
using System.Reactive.Kql;
using System.Reactive.Kql.CustomTypes;
using System.Collections.Generic;

namespace PowerShell.Kql
{

    [Cmdlet(VerbsCommunications.Read, "Winlog")]
    public class ReadWinlog : Cmdlet
    {
        [Parameter(Mandatory = false, Position = 0)]
        public string Log;

        [Parameter(Mandatory = false)]
        public string File;

        [Parameter(Mandatory = false)]
        public string Query;

        protected override void ProcessRecord()
        {
            if(Log != null)
            {
                Console.WriteLine("Reading the OS log: {0}", Log);
            }
            else
            {
                Console.WriteLine("Reading local file: {0}", File);
            }
        }
    }

}

