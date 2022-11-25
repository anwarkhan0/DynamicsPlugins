using System;
using System.Collections;
using System.Text;
using Microsoft.Xrm.Sdk;
using System.Web;
using System.Xml;
using System.Diagnostics;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Crm.Sdk.Messages;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace Work
{
    public class OpportunityNumberGen : IPlugin
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
                Entity Opportunity = (Entity)context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    if (Opportunity.Attributes.Contains("parentaccountid"))
                    {
                        EntityReference Account = (EntityReference)Opportunity.Attributes["parentaccountid"];
                        EntityCollection AccountOpportunities = GetRelatedOp(service, Account.Id);

                        string OpNumStr;
                        int OpNumInt;

                        if (AccountOpportunities.Entities[0] != null && AccountOpportunities.Entities[0].Attributes.Contains("new_opportunitynumber"))
                        {
                            OpNumStr = AccountOpportunities.Entities[0].Attributes["new_opportunitynumber"].ToString();
                            string[] subs = OpNumStr.Split('-');
                            OpNumInt = int.Parse(subs[1]);
                            OpNumInt++;

                        }
                        else
                        {
                            OpNumInt = 1;
                        }

                        string updateOpNum = "Op-" + String.Format("{0:000}", OpNumInt);
                        Opportunity["new_opportunitynumber"] = updateOpNum;
                        if (context.Depth > 1)
                        {
                            service.Update(Opportunity);
                        }


                    }

                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }

        public static EntityCollection GetRelatedOp(IOrganizationService service, Guid AccountID)
        {
            var query = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='opportunity'>
                <attribute name='name' />
                <attribute name='customerid' />
                <attribute name='estimatedvalue' />
                <attribute name='statuscode' />
                <attribute name='opportunityid' />
                <attribute name='new_opportunitynumber' />
                <order attribute='new_opportunitynumber' descending='true' />
                <filter type='and'>
                  <condition attribute='parentaccountid' operator='eq' uiname='ssc1' uitype='account' value='{AccountID}' />
                </filter>
              </entity>
            </fetch>";

            var Opportunities = service.RetrieveMultiple(new FetchExpression(query));
            return Opportunities;
        }
    }
}
