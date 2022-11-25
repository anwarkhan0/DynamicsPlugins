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
    public class StatusCreate : IPlugin
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
                Entity status = (Entity)context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    bool defaultOption = (bool)status["cr651_defaultoption1"];
                    
                    ConditionExpression condition1 = new ConditionExpression
                    {
                        AttributeName = "cr651_defaultoption1",
                        Operator = ConditionOperator.Equal
                    };
                    //condition to skip the current status
                    ConditionExpression condition2 = new ConditionExpression
                    {
                        AttributeName = "cr651_statusid",
                        Operator = ConditionOperator.NotEqual
                    };

                    condition1.Values.Add(true);
                    condition2.Values.Add(status.Id);
                    FilterExpression filter1 = new FilterExpression();
                    FilterExpression filter2 = new FilterExpression();
                    filter1.Conditions.Add(condition1);
                    filter2.Conditions.Add(condition2);
                    QueryExpression query = new QueryExpression("cr651_status");
                    query.ColumnSet.AddColumns("cr651_defaultoption1");
                    query.Criteria.AddFilter(filter1);
                    query.Criteria.AddFilter(filter2);
                    EntityCollection defaultRecs = service.RetrieveMultiple(query);

                    QueryExpression CountQuery = new QueryExpression("cr651_status");
                    CountQuery.ColumnSet = new ColumnSet();
                    CountQuery.Distinct = true;
                    CountQuery.ColumnSet.AddColumn("cr651_statusid");
                    CountQuery.PageInfo = new PagingInfo();
                    CountQuery.PageInfo.Count = 5000;
                    CountQuery.PageInfo.PageNumber = 1;
                    CountQuery.PageInfo.ReturnTotalRecordCount = true;
                    EntityCollection entityCollection = service.RetrieveMultiple(CountQuery);
                    int totalCount = entityCollection.Entities.Count;
                    if (totalCount == 0)
                    {
                        
                        //no record make it default
                        status["cr651_defaultoption1"] = true;
                    }
                    else if (defaultOption)
                    {
                        //if default selected to true
                        foreach (var c in defaultRecs.Entities)
                        {
                            
                            c["cr651_defaultoption1"] = false;
                            service.Update(c);
                        }
                        
                        status["cr651_defaultoption1"] = true;
                        
                    }
                    




                    //////////////////end try block////////////////////
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin." + ex.Message);
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
