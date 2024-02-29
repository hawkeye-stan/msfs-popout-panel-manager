namespace MSFSPopoutPanelManager.Shared
{
    public static class QualifyFullName
    {
        public static string Of(string _, [System.Runtime.CompilerServices.CallerArgumentExpression("_")] string fullTypeName = "")
        {
            if (fullTypeName.StartsWith("nameof(") && fullTypeName.EndsWith(")"))
            {
                return fullTypeName[7..^1];
            }
            return fullTypeName;
        }
    }
}
