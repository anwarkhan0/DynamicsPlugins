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
    public class ContactInformationUpdate : IPlugin
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
                    Entity PreContactInfo = (Entity)context.PreEntityImages["PreContactInfoImg"];
                    
                    if (ContactInfo.LogicalName == "cr651_contactinformation")
                    {
                        if (PreContactInfo.Attributes.Contains("cr651_customer"))
                        {
                            Guid contactId = ((EntityReference)PreContactInfo.Attributes["cr651_customer"]).Id;
                            ColumnSet attribList = new ColumnSet(new string[] {
                                "emailaddress1",
                                "telephone1"
                            });
                            Entity contact = service.Retrieve("contact", contactId, attribList);
                            if (ContactInfo.Attributes.Contains("cr651_contactmethod"))
                            {
                                OptionSetValue type = (OptionSetValue)ContactInfo["cr651_contactmethod"];
                                int contactType = type.Value;
                                if (contactType == 750340000)
                                {
                                    string email = contact.Attributes["emailaddress1"].ToString();
                                    ContactInfo["cr651_customeremail"] = email;
                                    ContactInfo["cr651_customerphone"] = "";
                                }
                                else
                                {
                                    string phone = contact.Attributes["telephone1"].ToString();
                                    ContactInfo["cr651_customerphone"] = phone;
                                    ContactInfo["cr651_customeremail"] = "";
                                }
                            }
                        }
                       
                        
                    }

                        //////////////////end try block////////////////////
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin." + ex.Message);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
