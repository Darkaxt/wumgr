using System;
using System.Collections.Generic;

namespace wumgr
{
    internal static class SettingsPageVisibilityPolicy
    {
        private const string HidePrefix = "hide:";
        private const string ShowOnlyPrefix = "showonly:";

        public static bool IsHidden(string currentValue, string page)
        {
            PageVisibilityList list = Parse(currentValue);
            if (list.Kind == PageVisibilityKind.ShowOnly)
                return !ContainsPage(list.Pages, page);

            return ContainsPage(list.Pages, page);
        }

        public static string SetHidden(string currentValue, string page, bool hidden)
        {
            PageVisibilityList list = Parse(currentValue);

            if (list.Kind == PageVisibilityKind.ShowOnly)
            {
                if (hidden)
                    list.Pages.RemoveAll(value => IsSamePage(value, page));
                else if (!ContainsPage(list.Pages, page))
                    list.Pages.Add(page);

                return ShowOnlyPrefix + string.Join(";", list.Pages.ToArray());
            }

            if (hidden)
            {
                if (!ContainsPage(list.Pages, page))
                    list.Pages.Add(page);
            }
            else
            {
                list.Pages.RemoveAll(value => IsSamePage(value, page));
            }

            if (list.Pages.Count == 0)
                return "";

            return HidePrefix + string.Join(";", list.Pages.ToArray());
        }

        private static PageVisibilityList Parse(string currentValue)
        {
            PageVisibilityList list = new PageVisibilityList();
            list.Pages = new List<string>();
            list.Kind = PageVisibilityKind.Hide;

            if (string.IsNullOrWhiteSpace(currentValue))
                return list;

            string value = currentValue.Trim();
            string pagesValue;
            if (value.StartsWith(HidePrefix, StringComparison.OrdinalIgnoreCase))
            {
                list.Kind = PageVisibilityKind.Hide;
                pagesValue = value.Substring(HidePrefix.Length);
            }
            else if (value.StartsWith(ShowOnlyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                list.Kind = PageVisibilityKind.ShowOnly;
                pagesValue = value.Substring(ShowOnlyPrefix.Length);
            }
            else
            {
                return list;
            }

            foreach (string entry in pagesValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string page = entry.Trim();
                if (page.Length != 0 && !ContainsPage(list.Pages, page))
                    list.Pages.Add(page.ToLowerInvariant());
            }

            return list;
        }

        private static bool ContainsPage(List<string> pages, string page)
        {
            foreach (string value in pages)
            {
                if (IsSamePage(value, page))
                    return true;
            }
            return false;
        }

        private static bool IsSamePage(string left, string right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private enum PageVisibilityKind
        {
            Hide,
            ShowOnly
        }

        private struct PageVisibilityList
        {
            public PageVisibilityKind Kind;
            public List<string> Pages;
        }
    }
}
