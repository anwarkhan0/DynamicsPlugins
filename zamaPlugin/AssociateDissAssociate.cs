using System;
using System.Collections;
using System.Text;
using Microsoft.Xrm.Sdk;
using System.Web;
using System.Xml;
using System.Diagnostics;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Crm.Sdk.Messages;
using System.Collections.Generic;
using System.Linq;

namespace zamaPlugin
{

    public class AssociateDissAssociate : IPlugin

    {

        public void Execute(IServiceProvider serviceProvider)

        {

            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.Depth > 1)
                return;

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            EntityReference targetEntity = null;
            string relationshipName = string.Empty;

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
            {
                targetEntity = context.InputParameters["Target"] as EntityReference;
            }
            if (context.InputParameters.Contains("Relationship"))
            {
                relationshipName = ((Relationship)context.InputParameters["Relationship"]).SchemaName;
            }

            //if the new subject is associated with teacher
            if (relationshipName == "cr651_cr651_teacher_cr651_subject")
            {
                Entity teacher = service.Retrieve(targetEntity.LogicalName, targetEntity.Id, new ColumnSet(true));
                if (context.InputParameters.Contains("RelatedEntities") && context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
                {
                    EntityCollection teacherSubjects = GetTeacherRelatedSubjects(service, teacher.Id);

                    //get the related student subjects
                    EntityCollection relatedStudents = GetTeacherRelatedStudents(service, teacher.Id);

                    if (context.MessageName.ToLower() == "associate" || context.MessageName.ToLower() == "disassociate")
                    {
                        foreach (var student in relatedStudents.Entities)
                        {
                            EntityCollection studentSubjects = GetStudentRelatedSubjects(service, student.Id);
                            //dissassociate the related old subjects from student
                            foreach (var subject in studentSubjects.Entities)
                            {
                                //Create a collection of the entity ids that will be associated to the student.
                                EntityReferenceCollection relatedEntities = new EntityReferenceCollection();
                                relatedEntities.Add(new EntityReference("cr651_subject", subject.Id));
                                //// Create an object that defines the relationship between the student and subjects.
                                Relationship relationship = new Relationship("cr651_cr651_subject_cr651_student");

                                ////Associate the contact with the accounts.
                                service.Disassociate(student.LogicalName, student.Id, relationship, relatedEntities);
                            }

                            //associate the relationship of each subject with student
                            foreach (var subject in teacherSubjects.Entities)
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
                }
            }
            //if cr651_cr651_teacher_cr651_subject End

            //if attempt to remove or add new subject to student
            if (relationshipName == "cr651_cr651_subject_cr651_student")
            {
                Entity student = service.Retrieve(targetEntity.LogicalName, targetEntity.Id, new ColumnSet("cr651_primaryteacher"));
                EntityReference teacher = (EntityReference)student["cr651_primaryteacher"];
                //stop adding and removing subjects if primary teacher is assigned to student
                if (context.InputParameters.Contains("RelatedEntities") && context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
                {
                    EntityReferenceCollection relatedRecords = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;
                    //EntityReference relatedEntityId = relatedRecords[0];

                    if (context.MessageName.ToLower() == "associate")
                    {
                        if (teacher != null)
                        {
                            throw new InvalidPluginExecutionException("No Additional Subjects can be added.");
                        }
                    }
                    else if (context.MessageName.ToLower() == "disassociate")
                    {
                        if (teacher != null)
                        {
                            throw new InvalidPluginExecutionException("Subjects cannot cannot be removed.");
                        }
                    }
                }
            }
            //if end

        }//end execute method

        public static EntityCollection GetTeacherRelatedSubjects(IOrganizationService service, Guid teacherRef)
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
                      <condition attribute='cr651_teacherid' operator='eq' uiname='Umar' uitype='cr651_teacher' value='{teacherRef}' />
                    </filter>
                  </link-entity>
                </link-entity>
              </entity>
            </fetch>";

            var result = service.RetrieveMultiple(new FetchExpression(fetch));

            return result;
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

        public static EntityCollection GetTeacherRelatedStudents(IOrganizationService service, Guid teacherId)
        {
            var fetch = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='cr651_student'>
                <attribute name='cr651_studentid' />
                <attribute name='cr651_name' />
                <attribute name='createdon' />
                <order attribute='cr651_name' descending='false' />
                <link-entity name='cr651_teacher' from='cr651_teacherid' to='cr651_primaryteacher' link-type='inner' alias='ad'>
                  <filter type='and'>
                    <condition attribute='cr651_teacherid' operator='eq' uiname='Umar' uitype='cr651_teacher' value='{teacherId}' />
                  </filter>
                </link-entity>
              </entity>
            </fetch>";

            var result = service.RetrieveMultiple(new FetchExpression(fetch));

            return result;
        }

    }





}