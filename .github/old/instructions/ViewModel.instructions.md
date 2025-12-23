---
applyTo: "**/*ViewModel.cs"
---
- The ViewModel should not contain any business logic.
- The ViewModel should always be associated with a MapperService to convert from the domain model.
- The ViewModel should only contain properties that are necessary for the view
- The ViewModel should be a record type to ensure immutability and value-based equality.
- The ViewModel should be colocated with the controller that uses it, in a folder named ViewModels.
