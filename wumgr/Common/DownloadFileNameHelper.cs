using System;
using System.IO;
using System.Text;

internal static class DownloadFileNameHelper
{
    public static string GetFileNameFromUri(Uri uri)
    {
        if (uri == null)
            return "";

        string path = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.ToString();
        try
        {
            path = Uri.UnescapeDataString(path);
        }
        catch
        {
        }

        return Sanitize(path);
    }

    public static string GetContentDispositionFileName(string contentDisposition)
    {
        if (string.IsNullOrWhiteSpace(contentDisposition))
            return null;

        string rawFileName = GetParameterValue(contentDisposition, "filename*");
        if (!string.IsNullOrWhiteSpace(rawFileName))
            rawFileName = DecodeExtendedFileName(rawFileName);

        if (string.IsNullOrWhiteSpace(rawFileName))
            rawFileName = GetParameterValue(contentDisposition, "filename");

        string fileName = Sanitize(rawFileName);
        return fileName.Length == 0 ? null : fileName;
    }

    public static string Sanitize(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "";

        fileName = fileName.Trim().Trim('"');
        int separatorIndex = fileName.LastIndexOfAny(new char[] { '\\', '/' });
        if (separatorIndex >= 0)
            fileName = fileName.Substring(separatorIndex + 1);

        if (string.IsNullOrWhiteSpace(fileName))
            return "";

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(invalidChar, '_');

        fileName = fileName.Trim().TrimEnd('.', ' ');
        if (fileName.Length == 0 || fileName == "." || fileName == "..")
            return "";

        return fileName;
    }

    private static string DecodeExtendedFileName(string rawFileName)
    {
        int firstQuote = rawFileName.IndexOf('\'');
        int secondQuote = firstQuote < 0 ? -1 : rawFileName.IndexOf('\'', firstQuote + 1);
        if (firstQuote >= 0 && secondQuote > firstQuote)
            rawFileName = rawFileName.Substring(secondQuote + 1);

        try
        {
            return Uri.UnescapeDataString(rawFileName);
        }
        catch
        {
            return rawFileName;
        }
    }

    private static string GetParameterValue(string header, string parameterName)
    {
        int index = 0;

        while (index < header.Length)
        {
            while (index < header.Length && (header[index] == ';' || char.IsWhiteSpace(header[index])))
                index++;

            int nameStart = index;
            while (index < header.Length && header[index] != '=' && header[index] != ';')
                index++;

            if (index >= header.Length || header[index] != '=')
            {
                while (index < header.Length && header[index] != ';')
                    index++;
                continue;
            }

            string name = header.Substring(nameStart, index - nameStart).Trim();
            index++;

            while (index < header.Length && char.IsWhiteSpace(header[index]))
                index++;

            string value;
            if (index < header.Length && header[index] == '"')
            {
                index++;
                StringBuilder builder = new StringBuilder();

                while (index < header.Length)
                {
                    char next = header[index++];
                    if (next == '\\')
                    {
                        if (index < header.Length && header[index] == '"')
                            builder.Append(header[index++]);
                        else
                            builder.Append(next);
                    }
                    else if (next == '"')
                    {
                        break;
                    }
                    else
                    {
                        builder.Append(next);
                    }
                }

                value = builder.ToString();
            }
            else
            {
                int valueStart = index;
                while (index < header.Length && header[index] != ';')
                    index++;

                value = header.Substring(valueStart, index - valueStart).Trim();
            }

            if (name.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
                return value;

            while (index < header.Length && header[index] != ';')
                index++;
        }

        return null;
    }
}
