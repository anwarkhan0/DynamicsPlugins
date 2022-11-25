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
    public class CreateSaleDetail : IPlugin
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
                Entity saleDetail = (Entity)context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {

                    decimal unitPrice = saleDetail.Attributes.Contains("cr651_unitprice") ? ((Money)saleDetail.Attributes["cr651_unitprice"]).Value : 0;
                    int quantity = saleDetail.Attributes.Contains("cr651_quantity") ? (int)saleDetail["cr651_quantity"] : 0;
                    decimal totalAmount = unitPrice * quantity;

                    Guid saleId = ((EntityReference)saleDetail.Attributes["cr651_saleinfo"]).Id;

                    ColumnSet attribList = new ColumnSet(new string[] {
                        "cr651_totalsaleamount"
                    });
                    Entity sale = service.Retrieve("cr651_sale", saleId, attribList);
                    decimal prevTotalAmount = sale.Attributes.Contains("cr651_totalsaleamount") ? ((Money)sale.Attributes["cr651_totalsaleamount"]).Value : 0;
                    decimal newTotalAmount = prevTotalAmount + totalAmount;
                    Money saleAmount = new Money
                    {
                        Value = newTotalAmount
                    };
                    sale["cr651_totalsaleamount"] = saleAmount;

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
