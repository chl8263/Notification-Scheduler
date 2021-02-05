Notification Scheduler
=============

:rocket: Document Purpose
-------------
This technical Design Document is for the Notification Shecduler application and provide technical resources with an explanation of the design and architecture and how to use this application.

<br/>

Approach
-------------
Some people use to make a lot of report and provide report for others. Also spend a lot of time for this repetitive task. This Program is perform routine task as scheduler and provide send mail function, File careation function and so on. Also Developer can add new task easly. The scheduler unit can be flexibly set to execute desired tasks. This program can save a lot of time for others.

<br/>

Scheduler Cycle
-------------

![image](https://user-images.githubusercontent.com/23304953/106612748-c8a57900-65ac-11eb-88ab-ab45852d7d22.png)

<br/>

Solution Design
-------------
Notification scheduler will be base on Window scheduler. Admin whoes manage this program need set scheduler on Window. The Notification scheduler itself run on the Window scheduler and determines wether to excute internal logic base on time data from database.
If trigger time is match, data inserted Task queue. Logic will be execute which passed time valid. If there is an error or system issue during the execute, the error message will inserted at System issue mail queue and that will be sent by email to system admin.

<br/>

Process Flow
-------------

![image](https://user-images.githubusercontent.com/23304953/106612974-0a362400-65ad-11eb-9156-55c2cbd9ff44.png)

This program get scheduler data and verify time check and insert to queue if time is valid. Then execute business logic that saved queue. During program execution, if occurred any issue or error, insert issue text at Issue email queue and send email to admin.

<br/>

Configuration Details
-------------

![image](https://user-images.githubusercontent.com/23304953/106613227-4ff2ec80-65ad-11eb-8217-9a4074f8cfd8.png)

<br/>

Program Engine, Util information
-------------

### 1. SchedulerManager 

-	Purpose : Scheduler Manager is main processing manager in this process. This class perform get scheduler data from Database and check whether time validate and insert queue when trigger time is match and execute data in the queue.

-	How to work : ‘GetBalidTaskQueue' method get scheduler data from data base and trigger time match and insert queue and return this valid queue. ‘ExecuteTaskQueue’ method is executing class from valid queue. When execute class, using C# reflection base on class name from Database’s class name column.

![image](https://user-images.githubusercontent.com/23304953/106613511-9e07f000-65ad-11eb-94c4-031f3115ce8d.png)

<br/>

### 2. MailHelper

-	Purpose : For Send mail more easily and if need to send email with file, can use attach file easily too.

-	How to work : There are two types send mail in this program.

1.	Send mail to admin
When occured system issue or error this program make report email and send mail to program manager. Also can choose email server Internal or external and email inforamtion in App.config file.

![image](https://user-images.githubusercontent.com/23304953/106615044-61d58f00-65af-11eb-9998-6542b1839fcd.png)

![image](https://user-images.githubusercontent.com/23304953/106615764-2091af00-65b0-11eb-9154-235b1204fe60.png)

The method specifiactions are as follows and file attachment takes the file path as an argument, access the path, and attaches it to the mail.

<br/>

### 2. Send mail to base on Scheduler data

When need to send mail based on Scheduler data. And can choose email server Internal or external and email inforamtion in App.config file.
The method specifiactions are as follows, get Scheduler data as an argument and send email using this information and file attachment takes the file path as an argument, access the path, and attaches it to the mail.

![image](https://user-images.githubusercontent.com/23304953/106616005-651d4a80-65b0-11eb-8c2f-875d6bfc38b0.png)

-	OutPut : Send email or send email with file.

<br/>

### 3. FileHelper

-	Purpose : For create CSV file easily and program manager can manage file easily.

-	How to work : There are some rule about create file in this program. First, create file and svae stage directory and some task using this file and should move file to complete directory.

![image](https://user-images.githubusercontent.com/23304953/106616166-95fd7f80-65b0-11eb-80ad-1c77e1f7bcca.png)

Create file function is make CSV file as C# Genewric, Reflection. So when develper use or add new CSV file, they just mark generic type as like below.

![image](https://user-images.githubusercontent.com/23304953/106616208-a4e43200-65b0-11eb-99c8-fe5a6c1d4daa.png)

Developer must follow the rules when create CSV file. 
The order of the VO list to create ad SCV file much match the order of SELECT in the query in DAO class cause use C# Reflection. Developer don’t have to do the complicated work of creating a CSV file as just follow this rules.

![image](https://user-images.githubusercontent.com/23304953/106616282-bb8a8900-65b0-11eb-8fb3-3eb924319720.png)

CreateCsvFileAtStage function is make file at path and return path and file name as Tuple.
Then move file to complete directory using MoveCompleteDirectory and use parameter from value which returned CreateCsvFileAtStage.

![image](https://user-images.githubusercontent.com/23304953/106616346-cd6c2c00-65b0-11eb-96b7-6f4ebc13855e.png)

<br/>

### 4. UrlHelper

-	Purpose : UrlHelper class help to make Url as type safe and make as Bulder pattern. So Developer can coding as good readability.

-	How to work : This class use term name follow under Url name.

![image](https://user-images.githubusercontent.com/23304953/106616529-01475180-65b1-11eb-84bf-9248b68fdb37.png)

<br/>

## How to add new logic

If Programmer want to add new business logic, make make new class file under ‘ScheduleTaskImpl’ directory and implement ‘ScheduleTask’ and overriding ‘Execute’ method.

![image](https://user-images.githubusercontent.com/23304953/106616820-508d8200-65b1-11eb-9bdf-f52020dcc939.png)

<br/>

![image](https://user-images.githubusercontent.com/23304953/106616862-5e430780-65b1-11eb-868d-c5fd2bc71124.png)

New Business logic class should be in ‘ScheduleTaskImpl’ directory. 
If need use constant, can add ‘Const’ directory.
If need use access Database, can add query at SAPDAO or can make DAO file in ‘DAO’ directory.
If need new value object, can create at ‘VO’ directory.

<br/>

Here is show how insert data in Data base for execute ‘NewBL’ class logic. The ‘ProgramClass’ column must match 'NewBL'.

```Sql
INSERT INTO [NotificationScheduler] (
    Department, Cycle, CycleDate, CycleDay, CycleTime, 
    EmailServer, EmailTitle, EmailContent, 
    ReceiverEmail, SenderName, SenderEmail, CcEmail, BccEmail, 
    ProgramClass, FirstRunDate, LastRunDate, Active, SpecialRun
) VALUES ( 
    'IT', 'ANY', NULL, NULL, NULL, 
    'INTERNAL', 'API server check', 'Univera SAP API Check program', 
    '*****@*****.com', '*****', 'IT@*****.com', NULL, NULL, 
    'NewBL', NULL, NULL, '1', '1'
); 
```
