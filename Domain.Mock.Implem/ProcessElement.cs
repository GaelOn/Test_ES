﻿using System;
using Domain.Base.Event;
using Domain.Base.Aggregate;
using Domain.Base.Event.EvantHandler;
using Domain.Mock.Implem.EventFromDomain.EntityOfInput;

namespace Domain.Mock.Implem
{
    public class ProcessElement : EntityBase<int, int>, IProcessElement
    {
        public string Name { get; private set; }
        public string RunningService { get; private set; }
        public DateTime Created { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime Stop { get; private set; }
        public ProcessElementState State { get; private set; }

        #region Constructor
        public ProcessElement(string name, int id, DateTime created) : base(id, new ProcessElementEventProcessor())
        {
            Name = name;
            Created = created;
            State = ProcessElementState.NotStartedYet;
            ((ProcessElementEventProcessor)EventHandlerMap).InEntity = this;
        }

        private void StartProcess(string runningService, DateTime start)
        {
            Start = start;
            RunningService = runningService;
            State = ProcessElementState.Running;
        }

        private void StopProcess(DateTime dateStop)
        {
            Stop = dateStop;
            State = ProcessElementState.Ended;
        }
        #endregion

        #region ProcessElementEventProcessor private class
        private class ProcessElementEventProcessor : BaseEventHandlerMap<int>
        {
            public ProcessElement InEntity { get; set; }

            public ProcessElementEventProcessor()
            {
                RegisterHandle(new DomainEventHandler<ProcessElemStarted, int>(HandleStartProcessElemEvent, RouteToEntity));
                RegisterHandle(new DomainEventHandler<ProcessElemStoped, int>(HandleStopProcessElemEvent, RouteToEntity));
            }

            public void HandleStartProcessElemEvent(ProcessElemStarted evt) => InEntity.StartProcess(evt.RunningService, evt.Start);
            public void HandleStopProcessElemEvent(ProcessElemStoped evt) => InEntity.StopProcess(evt.Stop);
            private void RouteToEntity(IDomainEventHandler<int> handler, IDomainEvent<int> evt)
                => InEntity.processEvent(handler, evt, evt.EventVersion + 1);
        }
        #endregion
    }
}