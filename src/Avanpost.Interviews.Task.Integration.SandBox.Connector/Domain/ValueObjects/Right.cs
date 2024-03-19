using System.Text;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector.Domain.ValueObjects
{
    public struct Right
    {
        public const string RequestRightGroupName = "Request";
        public const string ITRoleRightGroupName = "Role";
        public const string Delimeter = ":";

        public Right(RightKind kind, int id)
        {
            Kind = kind;
            Id = id;
        }

        public RightKind Kind { get; }
        public int Id { get; }

        public static Right Parse(string text)
        {
            var parts = text.Split(Delimeter);

            var id = int.Parse(parts[^1]);
            var kind = parts[0] == RequestRightGroupName
                ? RightKind.RequestRight
                : RightKind.ITRole;

            return new(kind, id);
        }

        public override string ToString()
        {
            return new StringBuilder()
                .Append(Kind is RightKind.ITRole ? ITRoleRightGroupName : RequestRightGroupName)
                .Append(Delimeter)
                .Append(Id)
                .ToString();
        }
    }
}