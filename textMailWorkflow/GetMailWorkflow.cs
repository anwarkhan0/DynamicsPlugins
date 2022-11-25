using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;


namespace textMailWorkflow
{
    public class GetMailWorkflow : CodeActivity
    {
        [Input("Key")]
        public InArgument<string> Key { get; set; }

        [Output("Tax")]
        public OutArgument<string> Tax { get; set; }

        protected override void Execute(CodeActivityContext ExContext)
        {
            ITracingService tracingService = ExContext.GetExtension<ITracingService>();

            //create the context
            IWorkflowContext context = ExContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)ExContext.GetExtension<IOrganizationService>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            string key = Key.Get(ExContext);

            QueryByAttribute query = new QueryByAttribute("cr651_configuration");
            query.ColumnSet = new ColumnSet(new string[] { "cr651_name" });
            query.AddAttributeValue("cr651_name", key);
            EntityCollection collection = service.RetrieveMultiple(query);

            if (collection.Entities.Count != 1)
            {
                tracingService.Trace("Something wrong with configuration");

            }

            Entity config = collection.Entities.FirstOrDefault();

            Tax.Set(ExContext, config.Attributes["cr651_value"].ToString());

        }
    }
}
