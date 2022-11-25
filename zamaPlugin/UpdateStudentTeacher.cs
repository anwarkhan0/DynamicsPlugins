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
    public class UpdateStudentTeacher : IPlugin
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
                Entity student = (Entity)context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    if (student.Attributes.Contains("cr651_primaryteacher"))
                    {
                        //get the teacher reference
                        EntityReference teacherRef = ((EntityReference)student["cr651_primaryteacher"]);
                        EntityCollection TeacherSubjects = GetTeacherRelatedSubjects(service, teacherRef);
                        EntityCollection StudentSubjects = GetStudentRelatedSubjects(service, student.Id);
                        if (context.MessageName == "Create")
                        {
                            //make the relationship of each subject with student
                            foreach (var subject in TeacherSubjects.Entities)
                            {
                                //Create a collection of the entity ids that will be associated to the student.
                                EntityReferenceCollection relatedEntities = new EntityReferenceCollection();
                                relatedEntities.Add(new EntityReference("cr651_subject", subject.Id));
                                //// Create an object that defines the relationship between the student and subjects.
                                Relationship relationship = new Relationship("cr651_cr651_subject_cr651_student");

                                ////Associate the contact with the accounts.
                                service.Associate(student.LogicalName, student.Id, relationship, relatedEntities);
                            }
                        }
                        if (context.MessageName == "Update")
                        {
                            //dissassociate the related old subjects from student
                            foreach (var subject in StudentSubjects.Entities)
                            {
                                //Create a collection of the entity ids that will be associated to the student.
                                EntityReferenceCollection relatedEntities = new EntityReferenceCollection();
                                relatedEntities.Add(new EntityReference("cr651_subject", subject.Id));
                                //// Create an object that defines the relationship between the student and subjects.
                                Relationship relationship = new Relationship("cr651_cr651_subject_cr651_student");

                                ////Associate the contact with the accounts.
                                service.Disassociate(student.LogicalName, student.Id, relationship, relatedEntities);
                            }

                            //make the relationship of each subject with student
                            foreach (var subject in TeacherSubjects.Entities)
                            {
                                //Create a collection of the entity ids that will be associated to the student.
                                EntityReferenceCollection relatedEntities = new EntityReferenceCollection();
                                relatedEntities.Add(new EntityReference("cr651_subject", subject.Id));
                                //// Create an object that defines the relationship between the student and subjects.
                                Relationship relationship = new Relationship("cr651_cr651_subject_cr651_student");

                                ////Associate the contact with the accounts.
                                service.Associate(student.LogicalName, student.Id, relationship, relatedEntities);
                            }
                        }
                    }

                    //get the teacher related subjects
                    //EntityCollection subjects = getTeacherRelatedSubjects(service, teacherRef);
                    ////loop through each object and associate relation with student
                    //foreach (var subject in subjects.Entities)
                    //{
                    //    EntityReference subjectRef = new EntityReference(subject.LogicalName, subject.Id);
                    //    AssociateSubjectsToStudents(subjectRef, teacherRef, service);
                    //}

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

        public static EntityCollection GetStudentRelatedSubjects(IOrganizationService service, Guid studentId)
        {
            var fetch = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
              <entity name='cr651_subject'>
                <attribute name='cr651_subjectid' />
                <attribute name='cr651_name' />
                <attribute name='createdon' />
                <order attribute='cr651_name' descending='false' />
                <link-entity name='cr651_cr651_subject_cr651_student' from='cr651_subjectid' to='cr651_subjectid' visible='false' intersect='true'>
                  <link-entity name='cr651_student' from='cr651_studentid' to='cr651_studentid' alias='ab'>
                    <filter type='and'>
                      <condition attribute='cr651_studentid' operator='eq' uiname='Umar' uitype='cr651_student' value='{studentId}' />
                    </filter>
                  </link-entity>
                </link-entity>
              </entity>
            </fetch>";

            var result = service.RetrieveMultiple(new FetchExpression(fetch));

            return result;
        }

        public static EntityCollection GetTeacherRelatedSubjects(IOrganizationService service, EntityReference teacherRef)
        {
            var fetch = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
              <entity name='cr651_subject'>
                <attribute name='cr651_subjectid' />
                <attribute name='cr651_name' />
                <attribute name='createdon' />
                <order attribute='cr651_name' descending='false' />
                <link-entity name='cr651_cr651_teacher_cr651_subject' from='cr651_subjectid' to='cr651_subjectid' visible='false' intersect='true'>
                  <link-entity name='cr651_teacher' from='cr651_teacherid' to='cr651_teacherid' alias='ab'>
                    <filter type='and'>
                      <condition attribute='cr651_teacherid' operator='eq' uiname='Umar' uitype='cr651_teacher' value='{teacherRef.Id}' />
                    </filter>
                  </link-entity>
                </link-entity>
              </entity>
            </fetch>";

            var result = service.RetrieveMultiple(new FetchExpression(fetch));

            return result;
        }

        //private EntityCollection getRelatedRecords(IOrganizationService service, string entity1, string entity2, string relationshipEntityName)
        //{

        //    QueryExpression query = new QueryExpression(entity1);
        //    query.ColumnSet = new ColumnSet(true);
        //    LinkEntity linkEntity1 = new LinkEntity(entity1, relationshipEntityName, , "{ Entity 1 Primary field }", JoinOperator.Inner);
        //    LinkEntity linkEntity2 = new LinkEntity(relationshipEntityName, entity2, "cr651_name", "{ Entity 2 Primary field }", JoinOperator.Inner);
        //    linkEntity1.LinkEntities.Add(linkEntity2);
        //    query.LinkEntities.Add(linkEntity1);
        //    // Add condition to match the Contact Name with “Arpit Shrivastava”
        //    linkEntity2.LinkCriteria = new FilterExpression();
        //    linkEntity2.LinkCriteria.AddCondition(new ConditionExpression("cr651_name", ConditionOperator.NotNull));
        //    EntityCollection recordcollection = service.RetrieveMultiple(query);
        //    return recordcollection;

        //}

        //public static void AssociateSubjectsToStudents(EntityReference subject, EntityReference student, IOrganizationService service)
        //{
        //    // Creating EntityReferenceCollection for the Contact
        //    EntityReferenceCollection relatedEntities = new EntityReferenceCollection();

        //    // Add the related entity contact
        //    relatedEntities.Add(subject);

        //    // Add the Account Contact relationship schema name
        //    Relationship relationship = new Relationship("cr651_cr651_subject_cr651_student");

        //    // Associate the contact record to Account
        //    service.Associate(student.LogicalName, student.Id, relationship, relatedEntities);

        //}
    }
}
