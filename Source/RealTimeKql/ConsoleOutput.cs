﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace RealTimeKql
{
    class ConsoleOutput: IObserver<IDictionary<string, object>>
    {
        private bool _running = false;
        private bool _error = false;
        private bool _firstEntry = true;
        private bool _tableFormat = false;

        public ConsoleOutput(bool tableFormat)
        {
            _running = true;
            _tableFormat = tableFormat;
        }

        public void OnNext(IDictionary<string, object> value)
        {
            if(_running)
            {
                if(_firstEntry && _tableFormat)
                {
                    _firstEntry = false;
                    Console.WriteLine(string.Join("\t", value.Keys));                    
                }

                // printing value to console
                if(_tableFormat)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach(var val in value.Values)
                    {
                        if(val.GetType() == typeof(Dictionary<string, object>))
                        {
                            sb.Append("Dictionary\t");
                        }
                        else
                        {
                            sb.Append($"{val}\t");
                        }
                    }
                    Console.WriteLine(sb.ToString());
                }
                else
                {
                    Console.WriteLine(JsonConvert.SerializeObject(value, Formatting.Indented));
                }
                
            }
        }

        public void OnError(Exception error)
        {
            this._error = true;
        }

        public void OnCompleted()
        {
            _running = false;
            if (_error != true)
            {
                Console.WriteLine("Completed!");
            }
        }
    }
}
