using System;
using Microsoft.Xrm.Sdk;

namespace PluginsCRM
{
    public class CrearPeticionDesdeRequerimiento : IPlugin
    {
        public void Execute()
        {
            // 1. Obtención de los servicios y contexto
            IPluginExecutionContext context = "";

            // 2. Validar mensaje y entidad
            if (context.MessageName.ToLower() != "create")
            {
                return;
            }

            if (!context.InputParameters.Contains("Target") || 
                !(context.InputParameters["Target"] is Entity))
            {
                return;
            }

            Entity incident = (Entity)context.InputParameters["Target"];

            if (incident.LogicalName.ToLower() != "incident")
            {
                return;
            }

            try
            {
                // 3. Preparar datos para la creación de la Petición

                // Owner de la Petición será el "ejecutivo resolutor" del Requerimiento
                EntityReference ownerEjecutivo = null;
                if (incident.Contains("new_ejecutivoresolutor") && 
                    incident["new_ejecutivoresolutor"] is EntityReference)
                {
                    ownerEjecutivo = (EntityReference)incident["new_ejecutivoresolutor"];
                }

                // Descripción del Requerimiento
                string descripcion = incident.Contains("description") 
                                    ? incident["description"].ToString() 
                                    : string.Empty;

                // Título o número de ticket
                string ticketNumber = incident.Contains("ticketnumber") 
                                      ? incident["ticketnumber"].ToString() 
                                      : string.Empty;

                // Subject (asunto) en la Petición.
                string tituloRequerimiento = incident.Contains("title")
                                             ? incident["title"].ToString()
                                             : string.Empty;

                // Fecha de resolución = 10 días después de la creación
                //    usamos DateTime.UtcNow
                DateTime fechaResolucion = DateTime.UtcNow.AddDays(10);

                // Referencia al Requerimiento creado
                EntityReference regardingRequerimiento = new EntityReference("incident", incident.Id);

                // crear la entidad Petición
                Entity peticion = new Entity("new_peticion");

                // Asignar la referencia al Requerimiento
                peticion["new_regarding"] = regardingRequerimiento;  // Ajustar schemaName si aplica

                //Asignar Owner = ejecutivo resolutor
                if (ownerEjecutivo != null)
                {
                    // El campo "Owner" en CRM es "ownerid"
                    peticion["ownerid"] = new EntityReference("systemuser", ownerEjecutivo.Id);
                }

                //Asignar el asunto
                peticion["subject"] = "Peticion";

                //Asignar la descripción (new_descripcion)
                peticion["new_descripcion"] = descripcion;

                //Asignar nombre (new_name) tomando el número de ticket, por ejemplo
                peticion["new_name"] = $"Petición - {ticketNumber}";

                //Asignar fecha de resolución (new_fechaderesolucion)
                peticion["new_fecharesolucion"] = fechaResolucion;

                //Crear la Petición en CRM
                //service.Create(peticion);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Ocurrió un error al crear la Petición.", ex);
            }
        }
    }
}
