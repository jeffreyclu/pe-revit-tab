#region autodesk libraries
using Autodesk.Revit.DB;
#endregion

#region system libraries
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace PERevitTab.Data
{
    public class SharepointConstants
    {
        public static class Cache
        {
            public static string username { get; set; }
            public static SecureString password { get; set; }
        }
        public static class Links
        {
            public static string siteUrl = @"https://perkinseastman.sharepoint.com/sites/Grove";
            public static string spReadList = @"cp_published_volumes";
            public static string spWriteList = @"cp_revit_volumes";
            public static string userListName = @"translatedUser";
            public static string projectListName = @"projects";
            public static string projectPermissionsListName = @"cp_project_permissions";
            public static string allItems = "All Items";
        }
        public static class ColumnHeaders
        {
            #region cp_published_volumes/cp_revit_volumes
            public static string org1_Title = "org1_Title";
            public static string org1_abbreviation = "org1_abbreviation";
            public static string org1_sort_id = "org1_sort_id";
            public static string org1_volume_sort = "org1_volume_sort";
            public static string org2_Title = "org2_Title";
            public static string org2_abbreviation = "org2_abbreviation";
            public static string org2_sort_id = "org2_sort_id";
            public static string org2_volume_sort = "org2_volume_sort";
            public static string org3_Title = "org3_Title";
            public static string org3_abbreviation = "org3_abbreviation";
            public static string org3_sort_id = "org3_sort_id";
            public static string org3_volume_sort = "org3_volume_sort";
            public static string org4_Title = "org4_Title";
            public static string org4_abbreviation = "org4_abbreviation";
            public static string org4_sort_id = "org4_sort_id";
            public static string org4_volume_sort = "org4_volume_sort";
            public static string org5_Title = "org5_Title";
            public static string org5_abbreviation = "org5_abbreviation";
            public static string org5_sort_id = "org5_sort_id";
            public static string org5_volume_sort = "org5_volume_sort";
            public static string org6_Title = "org6_Title";
            public static string org6_abbreviation = "org6_abbreviation";
            public static string org6_sort_id = "org6_sort_id";
            public static string org6_volume_sort = "org6_volume_sort";
            public static string org7_Title = "org7_Title";
            public static string org7_abbreviation = "org7_abbreviation";
            public static string org7_sort_id = "org7_sort_id";
            public static string org7_volume_sort = "org7_volume_sort";
            public static string vif_id = "vif_id";
            public static string is_depreciated = "is_depreciated";
            public static string pe_standard_Title = "pe_standard_Title";
            public static string leed_standard_Title = "leed_standard_Title";
            public static string ibc_standard_Title = "ibc_standard_Title";
            public static string gross_factor = "gross_factor";
            public static string area_net = "area_net";
            public static string count_occupant_capacity = "count_occupant_capacity";
            public static string count_occupant_non_capacity = "count_occupant_non_capacity";
            public static string count_volume_capacity = "count_volume_capacity";
            public static string count_volume_non_capacity = "count_volume_non_capacity";
            public static string phase_created_Title = "phase_created_Title";
            public static string phase_demolished_Title = "phase_demolished_Title";
            public static string vol_abbreviation = "vol_abbreviation";
            public static string vol_sort_id = "vol_sort_id";
            public static string vol_sorted = "vol_sorted";
            public static string vol_volume_sort = "vol_volume_sort";
            public static string comments = "comments";
            public static string Title = "Title";
            public static string id = "id";
            public static string vol_Title = "vol_Title";
            public static string revit_room_number = "revit_room_number";
            public static string revit_room_element_id = "revit_room_element_id";
            public static string revit_room_volume = "revit_room_volume";
            public static string revit_room_height = "revit_room_height";
            #endregion

            #region translatedUser
            public static string emailAddress = "emailAddress";
            public static string nickname = "nickname";
            #endregion
        }
        public static class Dictionaries
        {
            public static Dictionary<string, ParameterType> newRevitRoomParameters = new Dictionary<string, ParameterType>
            {
                { ColumnHeaders.org1_Title, ParameterType.Text },
                { ColumnHeaders.org1_abbreviation, ParameterType.Text },
                { ColumnHeaders.org1_sort_id, ParameterType.Integer },
                { ColumnHeaders.org1_volume_sort, ParameterType.Text },
                { ColumnHeaders.org2_Title, ParameterType.Text },
                { ColumnHeaders.org2_abbreviation, ParameterType.Text },
                { ColumnHeaders.org2_sort_id, ParameterType.Integer },
                { ColumnHeaders.org2_volume_sort, ParameterType.Text },
                { ColumnHeaders.org3_Title, ParameterType.Text },
                { ColumnHeaders.org3_abbreviation, ParameterType.Text },
                { ColumnHeaders.org3_sort_id, ParameterType.Integer },
                { ColumnHeaders.org3_volume_sort, ParameterType.Text },
                { ColumnHeaders.org4_Title, ParameterType.Text },
                { ColumnHeaders.org4_abbreviation, ParameterType.Text },
                { ColumnHeaders.org4_sort_id, ParameterType.Integer },
                { ColumnHeaders.org4_volume_sort, ParameterType.Text },
                { ColumnHeaders.org5_Title, ParameterType.Text },
                { ColumnHeaders.org5_abbreviation, ParameterType.Text },
                { ColumnHeaders.org5_sort_id, ParameterType.Integer },
                { ColumnHeaders.org5_volume_sort, ParameterType.Text },
                { ColumnHeaders.org6_Title, ParameterType.Text },
                { ColumnHeaders.org6_abbreviation, ParameterType.Text },
                { ColumnHeaders.org6_sort_id, ParameterType.Integer },
                { ColumnHeaders.org6_volume_sort, ParameterType.Text },
                { ColumnHeaders.org7_Title, ParameterType.Text },
                { ColumnHeaders.org7_abbreviation, ParameterType.Text },
                { ColumnHeaders.org7_sort_id, ParameterType.Integer },
                { ColumnHeaders.org7_volume_sort, ParameterType.Text },
                { ColumnHeaders.vif_id, ParameterType.Text },
                { ColumnHeaders.is_depreciated, ParameterType.YesNo },
                { ColumnHeaders.pe_standard_Title, ParameterType.Text },
                { ColumnHeaders.leed_standard_Title, ParameterType.Text },
                { ColumnHeaders.ibc_standard_Title, ParameterType.Text },
                { ColumnHeaders.gross_factor, ParameterType.Number },
                { ColumnHeaders.area_net, ParameterType.Number },
                { ColumnHeaders.count_occupant_capacity, ParameterType.Integer },
                { ColumnHeaders.count_occupant_non_capacity, ParameterType.Integer },
                { ColumnHeaders.count_volume_capacity, ParameterType.Integer },
                { ColumnHeaders.count_volume_non_capacity, ParameterType.Integer },
                { ColumnHeaders.phase_created_Title, ParameterType.Text },
                { ColumnHeaders.phase_demolished_Title, ParameterType.Text },
                { ColumnHeaders.vol_abbreviation, ParameterType.Text },
                { ColumnHeaders.vol_sort_id, ParameterType.Integer },
                { ColumnHeaders.vol_sorted, ParameterType.Text },
                { ColumnHeaders.vol_volume_sort, ParameterType.Text },
            };
        }
    }
}
