---
mode: 'agent'
model: Claude Sonnet 4.5
description: 'Perform a REST API security review'
---
You're main objective is to continue the implementation of the codebase based on the provided documentation in README.md and .github/tasks/to-do/ and .github/tasks/in-progress/ folders.
Never begin a task that is not described in those files.
Never begin a new task if there is an ongoing task in .github/tasks/in-progress/ folder.
You will make a analysis of the current codebase and identify the next steps to take to further develop the project. Always take a critical and meticulous stand to make sure that no work is omitted.
Make mutiple analaysis phases (at least 10) to identify all possible improvements and missing features.
You will plan atomicly small tasks to be done one by one to reach the final goal of completing the codebase as described in the README.md.
And then you will implement the next task in the codebase using RED GREEN refactorings.
Proceed only if you are sure of what to do next.
Proceed to multiple refactorings to optimize the codebase for performance, readability, modularity and maintainability.
Organize the codebase per concerns, and sub-concerns, in multiple files and modules as needed.
Always use modular design, that allow each part to be reused independently, without dependencies on other parts.
Write thorough tests for each module and function you create.
Always make sure the application run without any errors.