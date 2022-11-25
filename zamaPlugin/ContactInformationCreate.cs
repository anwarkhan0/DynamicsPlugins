using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace zamaPlugin
{
    public class ContactInformationCreate : IPlugin
    {
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity ContactInfo = (Entity)context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    Guid contactId = ((EntityReference)ContactInfo.Attributes["cr651_customer"]).Id;
                    OptionSetValue type = (OptionSetValue)ContactInfo["cr651_contactmethod"];
                    int contactType = type.Value;
                    ColumnSet attribList = new ColumnSet(new string[] {
                        "emailaddress1",
                        "telephone1"
                    });
                    Entity contact = service.Retrieve("contact", contactId, attribList);
                    if (contactType == 750340000)
                    {
                        if (contact.Attributes.Contains("emailaddress1"))
                        {
                            string email = contact.Attributes["emailaddress1"].ToString();
                            ContactInfo.Attributes["cr651_customeremail"] = email;
                        }
                        else
                        {
                            throw new InvalidPluginExecutionException("This Contact does not have Email.");
                        }
                        
                    }
                    else
                    {
                        if (contact.Attributes.Contains("telephone1"))
                        {
                            string phone = contact.Attributes["telephone1"].ToString();
                            ContactInfo.Attributes["cr651_customerphone"] = phone;
                        }
                        else {
                            throw new InvalidPluginExecutionException("This Contact does not have Phone Number.");
                        }
                        
                    }
                    if (context.Depth <= 1)
                    {
                        service.Update(ContactInfo);
                    }




                    //////////////////end try block////////////////////
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin."+ ex.Message);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.Message);
                    throw;
                }
            }
        }
    }
}
