using System.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;

namespace CourseCWF
{
    public class CourseCustomCreatePlugin : CodeActivity
    {
        [RequiredArgument]
        [Input("start date")]
        public InArgument<DateTime> StartDate { get; set; }
        [RequiredArgument]
        [Input("duration")]
        public InArgument<int> Duration { get; set; }

        [Output("end date")]
        public OutArgument<DateTime> EndDate { get; set; }

        protected override void Execute(CodeActivityContext Econtext)
        {
            IWorkflowContext context = Econtext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = Econtext.GetExtension<IOrganizationServiceFactory>();

            // Use the context service to create an instance of IOrganizationService.             
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);

            try
            {
                DateTime sDate = StartDate.Get(Econtext);
                int duration = Duration.Get(Econtext);
                var calendarRules = GetCalendarRules(service);
                List<DateTime> holidays = calendarRules.Select(rule => rule.GetAttributeValue<DateTime>("starttime")).Where(date => date >= DateTime.UtcNow).ToList();


                while (duration > 0)
                {
                    sDate = sDate.AddDays(1);
                    if (!holidays.Contains(sDate))
                    {
                        duration--;
                    }
                    else
                    {
                        throw new InvalidPluginExecutionException("holiday found");
                    }
                }

                //while (duration >= 0)
                //{

                //    if (sDate.DayOfWeek == DayOfWeek.Saturday || sDate.DayOfWeek == DayOfWeek.Sunday)
                //    {
                //        sDate = sDate.AddDays(1);
                //    }
                //    else
                //    {
                //        sDate = sDate.AddDays(1);
                //        duration--;
                //    }
                //}
                EndDate.Set(Econtext, sDate);
            }
            catch (Exception ex)
            {
                throw new InvalidWorkflowException(ex.Message);
            }

        }



        private List<Entity> GetCalendarRules(IOrganizationService sdk)
        {
            List<Entity> calendarRules = new List<Entity>();
            Entity businessClosureCalendar;

            QueryExpression q = new QueryExpression("calendar") { NoLock = true };
            q.Criteria.AddCondition("type", ConditionOperator.Equal, 2);
            q.Criteria.AddCondition("name", ConditionOperator.Equal, "Holidays");

            EntityCollection businessClosureCalendars = sdk.RetrieveMultiple(q);

            if (businessClosureCalendars.Entities.Count > 0)
            {
                businessClosureCalendar = businessClosureCalendars.Entities[0];
                calendarRules = businessClosureCalendar.GetAttributeValue<EntityCollection>("calendarrules").Entities.ToList();
            }

            return calendarRules;
        }

    }
}