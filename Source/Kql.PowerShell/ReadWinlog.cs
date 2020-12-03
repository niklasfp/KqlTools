using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reactive.Linq;
using Microsoft.EvtxEventXmlScrubber;

namespace Kql.PowerShell
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

        [Parameter(Mandatory = false)]
        public bool ReadExisting;

        protected override void ProcessRecord()
        {
            if(Log != null)
            {
                Console.WriteLine("Reading the OS log: {0}", Log);

                var etw = Tx.Windows.EvtxObservable.FromLog(Log, null, ReadExisting).Select(x => EvtxExtensions.Deserialize(x));
                ProcessStream(etw);
            }
            else
            {
                Console.WriteLine("Reading local file: {0}", File);
                // TODO: add in functionality to process local file
            }
        }

        void ProcessStream(IObservable<IDictionary<string, object>> events)
        {
            Console.WriteLine("Processing Stream...");
            if(Query != null)
            {
                // TODO: add in support for rx.kql using kqlnodev3
            }
            else
            {
                var enumerable = events.ToEnumerable();
                foreach (var e in enumerable)
                {
                    PSObject row = new PSObject();
                    foreach (var pair in e)
                    {
                        row.Properties.Add(new PSNoteProperty(pair.Key, pair.Value));
                    }
                    WriteObject(row);
                }
            }
        }
    }
}