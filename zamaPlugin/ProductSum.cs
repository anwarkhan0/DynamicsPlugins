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
    public class ProductSum : IPlugin
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

                    if (product.Attributes.Contains("new_producttype"))
                    {
                        Guid opId = ((EntityReference)product.Attributes["opportunityid"]).Id;
                        
                        ColumnSet attribList = new ColumnSet(new string[] {
                            "new_totallaptopsvalue",
                            "new_totalmobilesvalue"
                        });

                        
                        string productType = product.FormattedValues["new_producttype"].ToString();
                        Entity opportunity = service.Retrieve("opportunity", opId, attribList);

                        decimal perUnitPrice = ((Money)product.Attributes["priceperunit"]).Value;
                        decimal productQuantity = (Decimal)product.Attributes["quantity"];
                        
                        decimal totalAmount = perUnitPrice * productQuantity;

                        decimal totalLaptopVal = opportunity.Attributes.Contains("new_totallaptopsvalue") ? ((Money)opportunity.Attributes["new_totallaptopsvalue"]).Value : 0;
                        decimal totalMobileVal = opportunity.Attributes.Contains("new_totalmobilesvalue") ? ((Money)opportunity.Attributes["new_totalmobilesvalue"]).Value : 0;


                        if (productType == "Laptop")
                        {
                            decimal sumOfLaptopPrices = totalLaptopVal + totalAmount;
                            Money amount = new Money
                            {
                                Value = sumOfLaptopPrices
                            };
                            opportunity.Attributes["new_totallaptopsvalue"] = amount;
                        }
                        else if (productType == "Mobile")
                        {
                            decimal sumOfMobilesPrices = totalMobileVal + totalAmount;
                            Money amount = new Money
                            {
                                Value = sumOfMobilesPrices
                            };
                            opportunity.Attributes["new_totalmobilesvalue"] = amount;
                        }

                        if (context.Depth <= 1)
                        {
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
