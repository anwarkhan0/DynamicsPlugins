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
    public class AccountCopy : IPlugin
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
                context.InputParameters["Target"] is EntityReference)
            {
                // Obtain the target entity from the input parameters.  
                EntityReference accountRef = (EntityReference)context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                try
                {
                    //Get the attributes
                    Entity account = service.Retrieve(accountRef.LogicalName, accountRef.Id, new ColumnSet(true));
                    string accountName = account.Attributes.Contains("name") ? account["name"].ToString() : null;
                    string telephone1 = account.Attributes.Contains("telephone1") ? account["telephone1"].ToString() : null;
                    string websiteurl = account.Attributes.Contains("websiteurl") ? account["websiteurl"].ToString() : null;
                    string fax = account.Attributes.Contains("fax") ? account["fax"].ToString() : null;
                    EntityReference parentid = account.Attributes.Contains("parentaccountid") ? (EntityReference)account["parentaccountid"] : null;

                    Entity copyAccount = new Entity(accountRef.LogicalName);
                    if (accountName != null)
                    {
                        copyAccount.Attributes.Add("name", "copy-" + accountName);
                    }
                    if(telephone1 != null)
                    {
                        copyAccount.Attributes.Add("telephone1", telephone1);
                    }
                    if(websiteurl != null)
                    {
                        copyAccount.Attributes.Add("websiteurl", websiteurl);
                    }
                    if(fax != null)
                    {
                        copyAccount.Attributes.Add("fax", fax);
                    }
                    if (parentid != null)
                    {
                        copyAccount.Attributes.Add("parentaccountid", parentid);
                    }

                    service.Create(copyAccount);
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin.", ex);
                }
            }
            else
            {
                tracingService.Trace("No entity reference.");
            }
        }
    }

    
}
