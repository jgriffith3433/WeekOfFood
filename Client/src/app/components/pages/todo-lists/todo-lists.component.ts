import { Component, OnInit } from '@angular/core';
import { TodoListDTO } from '../../../models/TodoListDTO';
import { TodoListsService } from '../../../providers/todo-lists.service';

@Component({
  selector: 'app-todo-list',
  templateUrl: './todo-lists.component.html',
  styleUrls: ['./todo-lists.component.scss']
})
export class TodoListsComponent implements OnInit {
  todoLists: TodoListDTO[] = [];

  constructor(private todoListsService: TodoListsService) { }

  ngOnInit(): void {
    this.todoListsService.getAll().subscribe((x) => {
      this.todoLists = x;
    });
  }
}
