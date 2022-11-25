using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Tooling.Connector;

namespace zamaPlugin
{
    public class SendMailOnContactCreate : IPlugin
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
                Entity contact = (Entity)context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                        // Get a system user to send the email (From: field) mistake
                        //WhoAmIRequest systemUserRequest = new WhoAmIRequest();
                        //WhoAmIResponse systemUserResponse = (WhoAmIResponse)service.Execute(systemUserRequest);
                    Guid userId = context.UserId;
                    Guid contactId = contact.Id;
                    //Welcome Template ID = {30EAC211-F63F-ED11-9DB0-000D3A8BD8D8}

                    if (contact.Attributes.Contains("emailaddress1"))
                    {
                        //send the email from this current user
                        Entity fromActivityParty = new Entity("activityparty");
                        //to this contact user
                        Entity toActivityParty = new Entity("activityparty");

                        fromActivityParty["partyid"] = new EntityReference("systemuser", userId);
                        toActivityParty["partyid"] = new EntityReference("contact", contactId);

                        Entity email = new Entity("email");
                        email["from"] = new Entity[] { fromActivityParty };
                        email["to"] = new Entity[] { toActivityParty };
                        email["directioncode"] = true;
                        Guid templateId = new Guid("30EAC211-F63F-ED11-9DB0-000D3A8BD8D8");


                        SendEmailFromTemplateRequest emailUsingTemplateReq = new SendEmailFromTemplateRequest
                        {
                            // The Email Object created
                            Target = email,

                            // The Email Template Id
                            TemplateId = templateId,

                            // Template Regarding Record Id
                            RegardingId = contactId,

                            //Template Regarding Record’s Logical Name
                            RegardingType = contact.LogicalName
                        };
                        SendEmailFromTemplateResponse emailUsingTemplateResp = (SendEmailFromTemplateResponse)service.Execute(emailUsingTemplateReq);
                        Guid _emailId = emailUsingTemplateResp.Id;

                        tracingService.Trace(_emailId.ToString());

                        // We will get the contact id for Bob Smith using Retrieve
                        //ConditionExpression conditionExpression = new ConditionExpression();
                        //conditionExpression.AttributeName = "firstname";
                        //conditionExpression.Operator = ConditionOperator.Equal;
                        //conditionExpression.Values.Add("anwar");
                        //FilterExpression filterExpression = new FilterExpression();
                        //filterExpression.Conditions.Add(conditionExpression);
                        //QueryExpression query = new QueryExpression("contact");
                        //query.ColumnSet.AddColumns("contactid");
                        //query.Criteria.AddFilter(filterExpression);

                        //EntityCollection entityCollection = service.RetrieveMultiple(query);
                        //foreach (var a in entityCollection.Entities)
                        //{
                        //    //send the email from this current user
                        //    Entity fromActivityParty = new Entity("activityparty");
                        //    //to this contact user
                        //    Entity toActivityParty = new Entity("activityparty");

                        //    Guid contactId = (Guid)a.Attributes["contactid"];

                        //    fromActivityParty["partyid"] = new EntityReference("systemuser", userId);
                        //    toActivityParty["partyid"] = new EntityReference("contact", contactId);

                        //    Entity email = new Entity("email");
                        //    email["from"] = new Entity[] { fromActivityParty };
                        //    email["to"] = new Entity[] { toActivityParty };
                        //    email["regardingobjectid"] = new EntityReference("contact", contactId);
                        //    email["subject"] = "This is the subject";
                        //    email["description"] = "This is the description.";
                        //    email["directioncode"] = true;
                        //    Guid emailId = service.Create(email);


                        //    //Use the SendEmail message to send an e-mail message.
                        //   SendEmailRequest sendEmailRequest = new SendEmailRequest
                        //   {
                        //       EmailId = emailId,
                        //       TrackingToken = "",
                        //       IssueSend = true
                        //   };

                        //    SendEmailResponse sendEmailresp = (SendEmailResponse)service.Execute(sendEmailRequest);
                        //    tracingService.Trace("email sent");
                        //}

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

        private Entity GetEmailObject(Guid userId, Guid contactId)
        {
            throw new NotImplementedException();
        }
    }
}
