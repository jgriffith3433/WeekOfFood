﻿namespace ContainerNinja.Contracts.Data.Entities
{
    public class TodoList : AuditableEntity
    {
        public string Title { get; set; }
        public string Color { get; set; }
    }
}