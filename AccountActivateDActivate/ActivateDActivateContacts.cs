
using System.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace AccountActivateDActivate
{
    public class ActivateDActivateContacts : CodeActivity
    {
        //[RequiredArgument]
        //[Input("start date")]
        //public InArgument<Entity> Acount { get; set; }

        [Output("flag")]
        public OutArgument<bool> Flag { get; set; }
        //public void updateChargeState(Guid chargeId)
        //{

        //    SetStateRequest setState = new SetStateRequest();

        //    setState.EntityMoniker = new EntityReference();

        //    setState.EntityMoniker.Id = chargeId;

        //    setState.EntityMoniker.Name = "Charge";

        //    setState.EntityMoniker.LogicalName = new_charge.EntityLogicalName;

        //    setState.State = new OptionSetValue(1);
        //    setState.Status = new OptionSetValue(951850001);
        //    SetStateResponse setStateResponse = (SetStateResponse)service.Execute(setState);
        //}

        protected override void Execute(CodeActivityContext Econtext)
        {
            IWorkflowContext context = Econtext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = Econtext.GetExtension<IOrganizationServiceFactory>();

            // Use the context service to create an instance of IOrganizationService.             
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);

            Entity account = (Entity)context.InputParameters["Target"];

            try
            {
                OptionSetValue state = (OptionSetValue)account.Attributes["statecode"];
                //OptionSetValue status = (OptionSetValue)account.Attributes["statuscode"];
                int stateVal = state.Value;
                //int statusVal = status.Value;

                var Contacts = GetContactsBasedOnParentAccount(service, account.Id);
                if (Contacts != null)
                {
                    if (stateVal == 0)
                    {
                        foreach (var contact in Contacts.Entities)
                        {
                            Entity updateContact = new Entity(contact.LogicalName, contact.Id);
                            updateContact["statecode"] = new OptionSetValue(0);
                            updateContact["statuscode"] = new OptionSetValue(1);

                            service.Update(updateContact);
                        }
                    }
                    else if (stateVal == 1)
                    {
                        foreach (var contact in Contacts.Entities)
                        {
                            Entity updateContact = new Entity(contact.LogicalName, contact.Id);
                            updateContact["statecode"] = new OptionSetValue(1);
                            updateContact["statuscode"] = new OptionSetValue(2);

                            service.Update(updateContact);
                        }
                    }

                }
                Flag.Set(Econtext, true);
                //if (statusVal == 2)
                //{

                //    //activate all contacts related to this account
                //    //ConditionExpression condition = new ConditionExpression
                //    //{
                //    //    AttributeName = "parentcustomerid",
                //    //    Operator = ConditionOperator.Equal
                //    //};
                //    //condition.Values.Add(account.Id);
                //    //FilterExpression filter1 = new FilterExpression();
                //    //filter1.Conditions.Add(condition);
                //    //QueryExpression query = new QueryExpression("contact");
                //    //query.ColumnSet.AddColumns("statecode", "statuscode");
                //    //query.Criteria.AddFilter(filter1);
                //    //EntityCollection contacts = service.RetrieveMultiple(query);

                //    var Contacts = GetContactsBasedOnParentAccount(service, account.Id);
                //    if (Contacts != null)
                //    {
                //        if(stateVal == 0)
                //        {
                //            foreach (var contact in Contacts.Entities)
                //            {
                //                contact.Attributes["statecode"] = new OptionSetValue(0);
                //                contact.Attributes["statuscode"] = new OptionSetValue(1);

                //                service.Update(contact);
                //            }
                //        }else if(stateVal == 1)
                //        {
                //            foreach (var contact in Contacts.Entities)
                //            {
                //                contact.Attributes["statecode"] = new OptionSetValue(1);
                //                contact.Attributes["statuscode"] = new OptionSetValue(2);

                //                service.Update(contact);
                //            }
                //        }

                //    }

                //    foreach (Entity contact in contacts.Entities)
                //    {
                //        //OptionSetValue cstate = (OptionSetValue)contact.Attributes["statecode"];
                //        //OptionSetValue cstatus = (OptionSetValue)contact.Attributes["statuscode"];
                //        //throw new InvalidWorkflowException(cstate.Value.ToString() + ",,,," + cstatus.Value.ToString());
                //        //StateCode = 1 and StatusCode = 2 for deactivating Account or Contact
                //        //SetStateRequest setStateRequest = new SetStateRequest()

                //        //{
                //        //    EntityMoniker = new EntityReference
                //        //    {

                //        //        Id = contact.Id,
                //        //        LogicalName = "contact",

                //        //    },
                //        //    State = new OptionSetValue(1),
                //        //    Status = new OptionSetValue(2)
                //        //};

                //        //SetStateResponse setStateResponse = (SetStateResponse)service.Execute(setStateRequest);

                //        contact["statecode"] = new OptionSetValue(1);
                //        contact["statuscode"] = new OptionSetValue(2);

                //        service.Update(contact);
                //    }


                //}
                //else if (statusVal == 1)
                //{
                //    //De Activate all contacts
                //    ConditionExpression condition = new ConditionExpression
                //    {
                //        AttributeName = "parentcustomerid",
                //        Operator = ConditionOperator.Equal
                //    };
                //    condition.Values.Add(account.Id);
                //    FilterExpression filter1 = new FilterExpression();
                //    filter1.Conditions.Add(condition);
                //    QueryExpression query = new QueryExpression("contact");
                //    query.ColumnSet.AddColumns("statecode", "statuscode");
                //    query.Criteria.AddFilter(filter1);
                //    EntityCollection contacts = service.RetrieveMultiple(query);

                //    foreach (Entity contact in contacts.Entities)
                //    {
                //        //OptionSetValue cstate = (OptionSetValue)contact.Attributes["statecode"];
                //        //OptionSetValue cstatus = (OptionSetValue)contact.Attributes["statuscode"];
                //        //throw new InvalidWorkflowException(cstate.Value.ToString() + ",,,," + cstatus.Value.ToString());
                //        //StateCode = 1 and StatusCode = 2 for deactivating Account or Contact
                //        //SetStateRequest setStateRequest = new SetStateRequest()

                //        //{
                //        //    EntityMoniker = new EntityReference
                //        //    {

                //        //        Id = contact.Id,
                //        //        LogicalName = "contact",

                //        //    },
                //        //    State = new OptionSetValue(0),
                //        //    Status = new OptionSetValue(1)
                //        //};
                //        //_ = service.Execute(setStateRequest);
                //        //SetStateResponse setStateResponse = (SetStateResponse)service.Execute(setStateRequest);
                //        contact.Attributes["statecode"] = new OptionSetValue(0);
                //        contact.Attributes["statuscode"] = new OptionSetValue(1);
                //        service.Update(contact);
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw new InvalidWorkflowException(ex.Message);
            }

        }

        public static EntityCollection GetContactsBasedOnParentAccount (IOrganizationService service,  Guid AccountId)
        {
            var fetch = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                <attribute name='contactid' />

                                <filter type='and'>
                                  <condition attribute='parentcustomerid' operator='eq'  value='{AccountId}' />
                                </filter>
                              </entity>
                            </fetch>";

            var result = service.RetrieveMultiple(new FetchExpression(fetch));

            return result;
        }
    }
}