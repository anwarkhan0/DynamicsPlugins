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
    public class ShareAccountPlugin : IPlugin
    {

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is EntityReference)
            {
                

                try
                {
                    // Obtain the target entity from the input parameters.  
                    EntityReference account = (EntityReference)context.InputParameters["Target"];

                    //Obtain the principal access object from the input parameter
                    PrincipalAccess PrincipalAccess = (PrincipalAccess)context.InputParameters["PrincipalAccess"];

                    var userOrTeam = ((EntityReference)PrincipalAccess.Principal).Id;


                    //get the related contacts
                    EntityCollection relatedContacts = GetContactsBasedOnParentAccount(service, account.Id);

                    //if the message access share related contacts
                    if(context.MessageName == "GrantAccess")
                    {
                        foreach(var contact in relatedContacts.Entities)
                        {
                            ShareRelatedContacts(service, "contact", contact.Id, userOrTeam);
                        }
                    }

                    //if the message is revoke all the contacts permissions
                    if(context.MessageName == "RevokeAccess")
                    {
                        foreach (var contact in relatedContacts.Entities)
                        {
                            RevokeSharedContacts(service, "contact", contact.Id, userOrTeam);
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



        public static EntityCollection GetContactsBasedOnParentAccount(IOrganizationService service, Guid AccountId)
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

        public static void ShareRelatedContacts( IOrganizationService OrgService, string entityName, Guid recordId, Guid UserId )
        {
            EntityReference recordRef = new EntityReference(entityName, recordId);
            EntityReference User = new EntityReference("systemusers", UserId);

            //If its a team, Principal should be supplied with the team
            //EntityReference Team = new EntityReference(Team.EntityLogicalName, teamId);

            GrantAccessRequest grantAccessRequest = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.AppendToAccess | AccessRights.DeleteAccess,
                    Principal = User
                    //Principal = Team
                },
                Target = recordRef
            };
            OrgService.Execute(grantAccessRequest);
        }

        public static void RevokeSharedContacts(IOrganizationService OrgService, string TargetEntityName, Guid TargetId, Guid UserId)
        {
            EntityReference target = new EntityReference(TargetEntityName, TargetId);
            EntityReference User = new EntityReference("systemusers", UserId);

            RevokeAccessRequest revokeAccessRequest = new RevokeAccessRequest
            {
                Revokee = User,
                Target = target
            };
            OrgService.Execute(revokeAccessRequest);
        }

        public static void RetrieveSharedUsers( IOrganizationService OrganizationService, EntityReference entityRef )
        {
            var accessRequest = new RetrieveSharedPrincipalsAndAccessRequest
            {
                Target = entityRef
            };
            var accessResponse = (RetrieveSharedPrincipalsAndAccessResponse)OrganizationService.Execute(accessRequest);
            foreach (var principalAccess in accessResponse.PrincipalAccesses)

            {
                // principalAccess.Principal.Id - User Id
            }
        }



    }
}
