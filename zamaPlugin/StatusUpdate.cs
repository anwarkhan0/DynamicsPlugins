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
    public class StatusUpdate : IPlugin
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
                Entity preStatus = (Entity)context.PreEntityImages["PreImgOfStatus"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    bool defaultOption = (bool)status["cr651_defaultoption1"];

                    //bool isDefault = (bool)preStatus["cr651_defaultoption1"];

                    ////throw new InvalidPluginExecutionException(defaultOption.ToString() + ",,,,,,,,,,,,,," + isDefault.ToString());

                    //if (isDefault)
                    //{
                    //    throw new InvalidPluginExecutionException("You cannot change default to false.");
                    //}

                    ConditionExpression condition1 = new ConditionExpression
                    {
                        AttributeName = "cr651_defaultoption1",
                        Operator = ConditionOperator.Equal
                    };


                    condition1.Values.Add(true);
                    FilterExpression filter1 = new FilterExpression();
                    filter1.Conditions.Add(condition1);
                    QueryExpression query = new QueryExpression("cr651_status");
                    query.ColumnSet.AddColumns("cr651_defaultoption1");
                    query.Criteria.AddFilter(filter1);
                    EntityCollection defaultRecs = service.RetrieveMultiple(query);

                    if (defaultOption)
                    {
                        //if default selected to true
                        foreach (var c in defaultRecs.Entities)
                        {

                            c["cr651_defaultoption1"] = false;
                            service.Update(c);
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
                    tracingService.Trace("FollowUpPlugin: {0}", ex.Message);
                    throw;
                }
            }
        }
    }
}
