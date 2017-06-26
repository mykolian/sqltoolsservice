﻿//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.SqlTools.ServiceLayer.DisasterRecovery;
using Microsoft.SqlTools.ServiceLayer.TaskServices;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SqlTools.ServiceLayer.UnitTests.DisasterRecovery
{
    public class BackupTests
    {
        private TaskMetadata taskMetaData = new TaskMetadata
        {
            ServerName = "server name",
            DatabaseName = "database name",
            Name = "Backup Database", 
            IsCancelable = true
        };

        /// <summary>
        /// Create and run a backup task
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task VerifyRunningBackupTask()
        {
            using (SqlTaskManager manager = new SqlTaskManager())
            {
                DisasterRecoveryService service = new DisasterRecoveryService();
                var mockBackupOperation = new Mock<IBackupOperation>();
                this.taskMetaData.Data = mockBackupOperation.Object;
                
                SqlTask sqlTask = manager.CreateTask(this.taskMetaData, service.BackupTaskAsync);
                Assert.NotNull(sqlTask);
                Task taskToVerify = sqlTask.RunAsync().ContinueWith(Task =>
                {
                    Assert.Equal(SqlTaskStatus.Succeeded, sqlTask.TaskStatus);
                });

                await taskToVerify;
            }
        }
        
        /// <summary>
        /// Create and run multiple backup tasks
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task VerifyRunningMultipleBackupTasks()
        {
            using (SqlTaskManager manager = new SqlTaskManager())
            {   
                DisasterRecoveryService service = new DisasterRecoveryService();
                var mockUtility = new Mock<IBackupOperation>();
                this.taskMetaData.Data = mockUtility.Object;

                SqlTask sqlTask = manager.CreateTask(this.taskMetaData, service.BackupTaskAsync);
                SqlTask sqlTask2 = manager.CreateTask(this.taskMetaData, service.BackupTaskAsync);
                Assert.NotNull(sqlTask);
                Assert.NotNull(sqlTask2);

                Task taskToVerify = sqlTask.RunAsync().ContinueWith(Task =>
                {
                    Assert.Equal(SqlTaskStatus.Succeeded, sqlTask.TaskStatus);
                });

                Task taskToVerify2 = sqlTask2.RunAsync().ContinueWith(Task =>
                {
                    Assert.Equal(SqlTaskStatus.Succeeded, sqlTask2.TaskStatus);
                });

                await Task.WhenAll(taskToVerify, taskToVerify2);
            }
        }

        /// <summary>
        /// Cancel a backup task
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task VerifyCancelBackupTask()
        {
            using (SqlTaskManager manager = new SqlTaskManager())
            {
                IBackupOperation backupOperation = new BackupOperationStub();                
                DisasterRecoveryService service = new DisasterRecoveryService();
                this.taskMetaData.Data = backupOperation;
                SqlTask sqlTask = manager.CreateTask(this.taskMetaData, service.BackupTaskAsync);
                Assert.NotNull(sqlTask);
                Task taskToVerify = sqlTask.RunAsync().ContinueWith(Task =>
                {
                    Assert.Equal(SqlTaskStatus.Canceled, sqlTask.TaskStatus);
                    Assert.Equal(sqlTask.IsCancelRequested, true);
                    manager.Reset();
                });

                manager.CancelTask(sqlTask.TaskId);
                await taskToVerify;
            }
        }

        /// <summary>
        /// Cancel multiple backup tasks
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task VerifyCancelMultipleBackupTasks()
        {
            using (SqlTaskManager manager = new SqlTaskManager())
            {
                IBackupOperation backupOperation = new BackupOperationStub();
                DisasterRecoveryService service = new DisasterRecoveryService();
                this.taskMetaData.Data = backupOperation;
                SqlTask sqlTask = manager.CreateTask(this.taskMetaData, service.BackupTaskAsync);
                SqlTask sqlTask2 = manager.CreateTask(this.taskMetaData, service.BackupTaskAsync);
                Assert.NotNull(sqlTask);
                Assert.NotNull(sqlTask2);

                Task taskToVerify = sqlTask.RunAsync().ContinueWith(Task =>
                {
                    Assert.Equal(SqlTaskStatus.Canceled, sqlTask.TaskStatus);
                    Assert.Equal(sqlTask.IsCancelRequested, true);
                    manager.Reset();
                });

                Task taskToVerify2 = sqlTask2.RunAsync().ContinueWith(Task =>
                {
                    Assert.Equal(SqlTaskStatus.Canceled, sqlTask2.TaskStatus);
                    Assert.Equal(sqlTask2.IsCancelRequested, true);
                    manager.Reset();
                });

                manager.CancelTask(sqlTask.TaskId);
                manager.CancelTask(sqlTask2.TaskId);

                await Task.WhenAll(taskToVerify, taskToVerify2);
            }
        }

        /// <summary>
        /// Create two backup tasks and cancel one task
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task VerifyCombinationRunAndCancelBackupTasks()
        {
            using (SqlTaskManager manager = new SqlTaskManager())
            {
                IBackupOperation backupOperation = new BackupOperationStub();
                DisasterRecoveryService service = new DisasterRecoveryService();
                this.taskMetaData.Data = backupOperation;
                SqlTask sqlTask = manager.CreateTask(this.taskMetaData, service.BackupTaskAsync);
                SqlTask sqlTask2 = manager.CreateTask(this.taskMetaData, service.BackupTaskAsync);
                Assert.NotNull(sqlTask);
                Assert.NotNull(sqlTask2);

                Task taskToVerify = sqlTask.RunAsync().ContinueWith(Task =>
                {
                    Assert.Equal(SqlTaskStatus.Canceled, sqlTask.TaskStatus);
                    Assert.Equal(sqlTask.IsCancelRequested, true);
                    manager.Reset();
                });

                Task taskToVerify2 = sqlTask2.RunAsync().ContinueWith(Task =>
                {
                    Assert.Equal(SqlTaskStatus.Succeeded, sqlTask2.TaskStatus);
                });

                manager.CancelTask(sqlTask.TaskId);
                await Task.WhenAll(taskToVerify, taskToVerify2);
            }
        }
    }
}
