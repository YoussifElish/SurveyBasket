namespace SurveyBasket.Errors
{
    public static class RoleErrors
    {
        public static readonly Error InavlidRole = new("Role.RoleNotFount", "Role Is not Found", StatusCodes.Status404NotFound);
        public static readonly Error InvalidPermissions = new("Role.InvalidPermissions", "Invalid Permissions", StatusCodes.Status400BadRequest);
        public static readonly Error DuplicatedRole = new("Role.DuplicatedRole", "Another Role With The Same Name is exist", StatusCodes.Status404NotFound);

    }
}
