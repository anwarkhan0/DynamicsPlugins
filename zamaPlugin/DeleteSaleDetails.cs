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
    public class DeleteSaleDetails : IPlugin
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
                EntityReference saleDetail = (EntityReference)context.InputParameters["Target"];
                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    Entity PreSaleDetails = (Entity)context.PreEntityImages["PreSaleDelete"];
                    
                    Guid saleId = ((EntityReference)PreSaleDetails.Attributes["cr651_saleinfo"]).Id;
                    ColumnSet attribList = new ColumnSet(new string[] {
                        "cr651_totalsaleamount"
                    });
                    Entity sale = service.Retrieve("cr651_sale", saleId, attribList);

                    ConditionExpression condition1 = new ConditionExpression
                    {
                        AttributeName = "cr651_saleinfo",
                        Operator = ConditionOperator.Equal
                    };
                    condition1.Values.Add(saleId);
                    FilterExpression filter1 = new FilterExpression();
                    filter1.Conditions.Add(condition1);
                    QueryExpression query = new QueryExpression("cr651_saledetail");
                    query.ColumnSet.AddColumns("cr651_unitprice", "cr651_quantity");
                    query.Criteria.AddFilter(filter1);
                    EntityCollection result1 = service.RetrieveMultiple(query);
                    decimal newTotalAmount = 0;
                    foreach (var c in result1.Entities)
                    {
                        decimal cUnitPrice = ((Money)c.Attributes["cr651_unitprice"]).Value;
                        decimal cQuantity = (int)c.Attributes["cr651_quantity"];
                        decimal ctotal = cUnitPrice * cQuantity;
                        newTotalAmount += ctotal;
                    }

                    Money saleAmount = new Money
                    {
                        Value = newTotalAmount
                    };
                    sale["cr651_totalsaleamount"] = newTotalAmount;

                    if (context.Depth <= 1)
                    {
                        service.Update(sale);
                    }


                    //////////////////end try block////////////////////
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
    }
}
