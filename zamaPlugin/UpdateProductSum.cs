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
    public class UpdateProductSum : IPlugin
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
                Entity product = (Entity)context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {

                    Entity OpProduct = service.Retrieve(product.LogicalName, product.Id, new ColumnSet(true));
                    //Entity preProduct = (Entity)context.PreEntityImages["PreImage"];

                    if (OpProduct.Attributes.Contains("new_producttype"))
                    {
                        Guid opId = ((EntityReference)OpProduct.Attributes["opportunityid"]).Id;

                        ColumnSet attribList = new ColumnSet(new string[] {
                            "new_totallaptopsvalue",
                            "new_totalmobilesvalue"
                        });


                        string productType = OpProduct.FormattedValues["new_producttype"].ToString();
                        Entity opportunity = service.Retrieve("opportunity", opId, attribList);

                        decimal perUnitPrice = ((Money)OpProduct.Attributes["priceperunit"]).Value;
                        decimal productQuantity = (Decimal)OpProduct.Attributes["quantity"];
                        //decimal preProductQuantity = (Decimal)preProduct.Attributes["quantity"];
                        
                        decimal totalAmount = perUnitPrice * productQuantity;

                        decimal totalLaptopVal = opportunity.Attributes.Contains("new_totallaptopsvalue") ? ((Money)opportunity.Attributes["new_totallaptopsvalue"]).Value : 0;
                        decimal totalMobileVal = opportunity.Attributes.Contains("new_totalmobilesvalue") ? ((Money)opportunity.Attributes["new_totalmobilesvalue"]).Value : 0;


                        if (productType == "Laptop")
                        {
                            ConditionExpression condition1 = new ConditionExpression
                            {
                                AttributeName = "new_producttype",
                                Operator = ConditionOperator.Equal
                            };
                            ConditionExpression condition2 = new ConditionExpression
                            {
                                AttributeName = "opportunityid",
                                Operator = ConditionOperator.Equal
                            };
                            condition1.Values.Add("100000000");
                            condition2.Values.Add(opId);
                            FilterExpression filter1 = new FilterExpression();
                            FilterExpression filter2 = new FilterExpression();
                            filter1.Conditions.Add(condition1);
                            filter2.Conditions.Add(condition2);
                            QueryExpression query = new QueryExpression("opportunityproduct");
                            query.ColumnSet.AddColumns("extendedamount");
                            query.Criteria.AddFilter(filter1);
                            query.Criteria.AddFilter(filter2);
                            EntityCollection result1 = service.RetrieveMultiple(query);

                            decimal sumOfLaptopPrices = 0;
                            foreach (var c in result1.Entities)
                            {
                                decimal extendedAmount = ((Money)c.Attributes["extendedamount"]).Value;
                                sumOfLaptopPrices += extendedAmount;
                            }
                            
                            Money amount = new Money
                            {
                                Value = sumOfLaptopPrices
                            };
                            opportunity.Attributes["new_totallaptopsvalue"] = amount;
                        }
                        else if (productType == "Mobile")
                        {
                            ConditionExpression condition1 = new ConditionExpression
                            {
                                AttributeName = "new_producttype",
                                Operator = ConditionOperator.Equal
                            };
                            ConditionExpression condition2 = new ConditionExpression
                            {
                                AttributeName = "opportunityid",
                                Operator = ConditionOperator.Equal
                            };
                            condition1.Values.Add("100000001");
                            condition2.Values.Add(opId);
                            FilterExpression filter1 = new FilterExpression();
                            FilterExpression filter2 = new FilterExpression();
                            filter1.Conditions.Add(condition1);
                            filter2.Conditions.Add(condition2);
                            QueryExpression query = new QueryExpression("opportunityproduct");
                            query.ColumnSet.AddColumns("extendedamount");
                            query.Criteria.AddFilter(filter1);
                            query.Criteria.AddFilter(filter2);
                            EntityCollection result1 = service.RetrieveMultiple(query);

                            decimal sumOfMobilesPrices = 0;
                            foreach (var c in result1.Entities)
                            {
                                decimal extendedAmount = ((Money)c.Attributes["extendedamount"]).Value;
                                sumOfMobilesPrices += extendedAmount;
                            }
                            Money amount = new Money
                            {
                                Value = sumOfMobilesPrices
                            };
                            opportunity.Attributes["new_totalmobilesvalue"] = amount;
                        }

                        if (context.Depth <= 1)
                        {
                            service.Update(OpProduct);
                            service.Update(opportunity);
                        }

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
