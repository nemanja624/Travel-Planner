namespace Contracts.Common;

public enum ActivityStatus
{
    Planned = 0,
    Reserved = 1,
    Completed = 2,
    Cancelled = 3
}

public enum ExpenseCategory
{
    Transport = 0,
    Accommodation = 1,
    Food = 2,
    Tickets = 3,
    Shopping = 4,
    Other = 5
}

public enum ShareAccessLevel
{
    View = 0,
    Edit = 1
}

public enum UserRole
{
    User = 0,
    Admin = 1
}
