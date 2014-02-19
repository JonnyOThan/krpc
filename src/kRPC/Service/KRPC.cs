﻿using System;
using System.Collections.Generic;
using Google.ProtocolBuffers;
using KRPC.Utils;

namespace KRPC.Service
{
    [KRPCService]
    class KRPC
    {
        public static IDictionary<string,ServiceSignature> Signatures { get; set; }

        [KRPCProcedure]
        public static Schema.KRPC.Status GetStatus ()
        {
            var status = Schema.KRPC.Status.CreateBuilder ();
            status.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return status.Build ();
        }

        [KRPCProcedure]
        public static Schema.KRPC.Services GetServices ()
        {
            if (Signatures == null)
                throw new RPCException ("Services have not been loaded");

            var services = Schema.KRPC.Services.CreateBuilder ();
            foreach (var serviceSignature in Signatures.Values) {
                var service = Schema.KRPC.Service.CreateBuilder ();
                service.SetName (serviceSignature.Name);
                foreach (var procedureSignature in serviceSignature.Procedures.Values) {
                    var procedure = Schema.KRPC.Procedure.CreateBuilder ();
                    procedure.Name = procedureSignature.Name;
                    if (procedureSignature.HasReturnType)
                        procedure.ReturnType = Reflection.GetMessageTypeName (procedureSignature.ReturnType);
                    //TODO: allow multiple parameters
                    if (procedureSignature.ParameterTypes.Count > 1)
                        throw new NotImplementedException();
                    if (procedureSignature.ParameterTypes.Count == 1)
                        procedure.ParameterType = Reflection.GetMessageTypeName (procedureSignature.ParameterTypes [0]);
                    service.AddProcedures (procedure);
                }
                services.AddServices_ (service);
            }
            Schema.KRPC.Services result = services.Build ();
            return result;
        }
    }
}
