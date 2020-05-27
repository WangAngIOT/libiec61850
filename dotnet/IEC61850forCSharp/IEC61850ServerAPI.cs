/*
 *  IEC61850ServerAPI.cs
 *
 *  Copyright 2016 Michael Zillgith
 *
 *  This file is part of libIEC61850.
 *
 *  libIEC61850 is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  libIEC61850 is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with libIEC61850.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  See COPYING file for the complete license text.
 */
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using IEC61850.Common;
using IEC61850.TLS;

// IEC 61850 API for the libiec61850 .NET wrapper library
namespace IEC61850
{
    // IEC 61850 server API.
    namespace Server
    {
        /// <summary>
        /// Config file parser.
        /// </summary>
        public class ConfigFileParser
        {
            [DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
            static extern IntPtr ConfigFileParser_createModelFromConfigFileEx(string filename);

            public static IedModel CreateModelFromConfigFile(string filePath)
            { 
                IntPtr retVal = ConfigFileParser_createModelFromConfigFileEx (filePath);
                if (retVal == IntPtr.Zero) {
                    return null;
                }

                return new IedModel (retVal);
            }
        }

        /// <summary>
        /// Representation of the IED server data model
        /// </summary>
		public class IedModel
		{
			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr IedModel_create(string name);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr IedModel_destroy(IntPtr self);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr IedModel_getModelNodeByObjectReference(IntPtr self, string objectReference);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr IedModel_getModelNodeByShortObjectReference(IntPtr self, string objectReference);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern int ModelNode_getType(IntPtr self);

			internal IntPtr self = IntPtr.Zero;

			internal IedModel(IntPtr self)
			{
				this.self = self;
			}

            /// <summary>
            /// Initializes a new instance of the <see cref="T:IEC61850.Server.IedModel"/> class.
            /// </summary>
            /// <param name="name">IED name</param>
			public IedModel(string name)
			{
				self = IedModel_create(name);
			}

			// causes undefined behavior
			//~IedModel()
			//{
			//    if (self != IntPtr.Zero)
			//    {
			//        IedModel_destroy(self);
			//    }
			//}

			public void Destroy()
			{
				IedModel_destroy(self);
				self = IntPtr.Zero;
			}

			public static IedModel CreateFromFile(string filePath)
			{
				return ConfigFileParser.CreateModelFromConfigFile(filePath);
			}

			private ModelNode getModelNodeFromNodeRef(IntPtr nodeRef)
			{				
				int nodeType = ModelNode_getType (nodeRef);

				switch (nodeType) {
				case 0:
					return new LogicalDevice (nodeRef);

				case 1:
					return new LogicalNode (nodeRef);

				case 2:
					return new DataObject (nodeRef);

				case 3:
					return new DataAttribute (nodeRef);

				default:
					return new ModelNode (nodeRef);
				}
			}

			public ModelNode GetModelNodeByObjectReference(string objectReference)
			{
				IntPtr nodeRef = IedModel_getModelNodeByObjectReference(self, objectReference);

				if (nodeRef == IntPtr.Zero)
					return null;

				return getModelNodeFromNodeRef (nodeRef);
			}

			public ModelNode GetModelNodeByShortObjectReference(string objectReference)
			{
				IntPtr nodeRef = IedModel_getModelNodeByShortObjectReference(self, objectReference);

				if (nodeRef == IntPtr.Zero)
					return null;

				return getModelNodeFromNodeRef (nodeRef);
			}
		}

		public class LogicalDevice : ModelNode
		{
			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr LogicalDevice_create(string name, IntPtr parent);

			public LogicalDevice (IntPtr self) : base (self)
			{
			}

			public LogicalDevice(string name, IedModel parent)
			{
				self = LogicalDevice_create(name, parent.self);
			}
		}

		public class LogicalNode : ModelNode
		{
			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr LogicalNode_create(string name, IntPtr parent);

			public LogicalNode (IntPtr self) : base(self)
			{
			}

			public LogicalNode(string name, LogicalDevice parent)
			{
				base.self = LogicalNode_create(name, parent.self);
			}
		}

		public enum AccessPolicy {
			ACCESS_POLICY_ALLOW = 0,
			ACCESS_POLICY_DENY = 1
		}

		public enum DataAttributeType {
			BOOLEAN = 0,
			INT8 = 1,
			INT16 = 2,
			INT32 = 3,
			INT64 = 4, 
			INT128 = 5, 
			INT8U = 6,
			INT16U = 7,
			INT24U = 8,
			INT32U = 9,
			FLOAT32 = 10,
			FLOAT64 = 11,
			ENUMERATED = 12,
			OCTET_STRING_64 = 13,
			OCTET_STRING_6 = 14,
			OCTET_STRING_8 = 15,
			VISIBLE_STRING_32 = 16,
			VISIBLE_STRING_64 = 17,
			VISIBLE_STRING_65 = 18,
			VISIBLE_STRING_129 = 19,
			VISIBLE_STRING_255 = 20,
			UNICODE_STRING_255 = 21,
			TIMESTAMP = 22,
			QUALITY = 23,
			CHECK = 24,
			CODEDENUM = 25,
			GENERIC_BITSTRING = 26,
			CONSTRUCTED = 27,
			ENTRY_TIME = 28,
			PHYCOMADDR = 29,
			CURRENCY = 30
		}

		public enum ModeValues
		{
			ON = 1,
			BLOCKED = 2,
			TEST = 3,
			TEST_BLOCKED = 4,
			OFF = 5
		}

		public enum HealthValues
		{
			OK = 1,
			WARNING = 2,
			ALARM = 3
		}

		public class CAC
		{
			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CAC_Unit_create(string name, IntPtr parent, bool hasMagnitude);

			public static DataAttribute Create_CAC_Unit(ModelNode parent, string name, bool hasMagnitude)
			{
				IntPtr self = CAC_Unit_create(name, parent.self, hasMagnitude);

				if (self != IntPtr.Zero)
					return new DataAttribute(self);
				else
					return null;
			}

		}

		/// <summary>
		/// The CDC class contains helper functions to create DataObject instances for the
		/// most common Common Data Classes.
		/// </summary>
		public class CDC 
		{
			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_SPS_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_DPS_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_VSS_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_SEC_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_CMV_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_SAV_create(string name, IntPtr parent, uint options, [MarshalAs(UnmanagedType.I1)] bool isIntegerNotFloat);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_ACD_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_ACT_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_SPG_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_VSG_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_ENG_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_ING_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_ASG_create(string name, IntPtr parent, uint options, [MarshalAs(UnmanagedType.I1)] bool isIntegerNotFloat);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_WYE_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_DEL_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_HST_create(string name, IntPtr parent, uint options, ushort maxPts);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_INS_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_MV_create(string name, IntPtr parent, uint options, [MarshalAs(UnmanagedType.I1)] bool isIntegerNotFloat);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_INC_create(string name, IntPtr parent, uint options, uint controlOptions);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_LPL_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_DPL_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_ENS_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_SPC_create(string name, IntPtr parent, uint options, uint controlOptions);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_DPC_create(string name, IntPtr parent, uint options, uint controlOptions);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_BSC_create(string name, IntPtr parent, uint options, uint controlOptions, [MarshalAs(UnmanagedType.I1)] bool hasTransientIndicator);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_APC_create(string name, IntPtr parent, uint options, uint controlOptions, [MarshalAs(UnmanagedType.I1)] bool isIntegerNotFloat);
			
			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_BCR_create(string name, IntPtr parent, uint options);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_ENC_create(string name, IntPtr parent, uint options, uint controlOptions);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_SPV_create(string name, IntPtr parent, uint options, uint controlOptions, uint wpOptions, [MarshalAs(UnmanagedType.I1)] bool hasChaManRs);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_STV_create(string name, IntPtr parent, uint options, uint controlOptions, uint wpOptions, [MarshalAs(UnmanagedType.I1)] bool hasOldStatus);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_CMD_create(string name, IntPtr parent, uint options, uint controlOptions, uint wpOptions, [MarshalAs(UnmanagedType.I1)] bool hasOldStatus, [MarshalAs(UnmanagedType.I1)] bool hasCmTm, [MarshalAs(UnmanagedType.I1)] bool hasCmCt);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_ALM_create(string name, IntPtr parent, uint options, uint controlOptions, uint wpOptions, [MarshalAs(UnmanagedType.I1)] bool hasOldStatus);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_CTE_create(string name, IntPtr parent, uint options, uint controlOptions, uint wpOptions, [MarshalAs(UnmanagedType.I1)] bool hasHisRs);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr CDC_TMS_create(string name, IntPtr parent, uint options, uint controlOptions, uint wpOptions, [MarshalAs(UnmanagedType.I1)] bool hasHisRs);

			public const int CDC_OPTION_DESC = (1 << 2);
			public const int CDC_OPTION_DESC_UNICODE = (1 << 3);
			public const int CDC_OPTION_AC_DLNDA = (1 << 4);
			public const int CDC_OPTION_AC_DLN = (1 << 5);
			public const int CDC_OPTION_UNIT = (1 << 6);

			public const int CDC_OPTION_MIN = (1 << 14);
			public const int CDC_OPTION_MAX = (1 << 15);

			// options that are only valid for DPL CDC
			public const int CDC_OPTION_DPL_HWREV = (1 << 17);
			public const int CDC_OPTION_DPL_SWREV = (1 << 18);
			public const int CDC_OPTION_DPL_SERNUM = (1 << 19);
			public const int CDC_OPTION_DPL_MODEL = (1 << 20);
			public const int CDC_OPTION_DPL_LOCATION = (1 << 21);

			// mandatory data attributes for LLN0 (e.g. LBL configRef)
			public const int CDC_OPTION_AC_LN0_M = (1 << 24);
			public const int CDC_OPTION_AC_LN0_EX = (1 << 25);
			public const int CDC_OPTION_AC_DLD_M = (1 << 26);


			public static DataObject Create_CDC_SPS(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_SPS_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject (self);
				else
					return null;
			}
			
			public static DataObject Create_CDC_DPS(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_DPS_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_VSS(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_VSS_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_SEC(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_SEC_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_CMV(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_CMV_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_SAV(ModelNode parent, string name, uint options, bool isIntegerNotFloat)
			{
				IntPtr self = CDC_SAV_create(name, parent.self, options, isIntegerNotFloat);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_ACD(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_ACD_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_ACT(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_ACT_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_SPG(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_SPG_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_VSG(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_VSG_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_ENG(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_ENG_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_ING(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_ING_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_ASG(ModelNode parent, string name, uint options, bool isIntegerNotFloat)
			{
				IntPtr self = CDC_ASG_create(name, parent.self, options, isIntegerNotFloat);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_WYE(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_WYE_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_DEL(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_DEL_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_HST(ModelNode parent, string name, uint options, ushort maxPts)
			{
				IntPtr self = CDC_HST_create(name, parent.self, options, maxPts);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_INS(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_INS_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject (self);
				else
					return null;
			}

			public static DataObject Create_CDC_MV(ModelNode parent, string name, uint options, bool isIntegerNotFloat)
			{
				IntPtr self = CDC_MV_create(name, parent.self, options, isIntegerNotFloat);

				if (self != IntPtr.Zero)
					return new DataObject (self);
				else
					return null;
			}

			public static DataObject Create_CDC_INC(ModelNode parent, string name, uint options, uint controlOptions)
			{
				IntPtr self = CDC_INC_create(name, parent.self, options, controlOptions);

				if (self != IntPtr.Zero)
					return new DataObject (self);
				else
					return null;
			}

			public static DataObject Create_CDC_LPL(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_LPL_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject (self);
				else
					return null;
			}

			public static DataObject Create_CDC_DPL(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_DPL_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject (self);
				else
					return null;
			}

			public static DataObject Create_CDC_ENS(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_ENS_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_SPC(ModelNode parent, string name, uint options, uint controlOptions)
			{
				IntPtr self = CDC_SPC_create(name, parent.self, options, controlOptions);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_DPC(ModelNode parent, string name, uint options, uint controlOptions)
			{
				IntPtr self = CDC_DPC_create(name, parent.self, options, controlOptions);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_BSC(ModelNode parent, string name, uint options, uint controlOptions, bool hasTransientIndicator)
			{
				IntPtr self = CDC_BSC_create(name, parent.self, options, controlOptions, hasTransientIndicator);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_APC(ModelNode parent, string name, uint options, uint controlOptions, bool isIntegerNotFloat)
			{
				IntPtr self = CDC_APC_create(name, parent.self, options, controlOptions, isIntegerNotFloat);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_BCR(ModelNode parent, string name, uint options)
			{
				IntPtr self = CDC_BCR_create(name, parent.self, options);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_ENC(ModelNode parent, string name, uint options, uint controlOptions)
			{
				IntPtr self = CDC_ENC_create(name, parent.self, options, controlOptions);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_SPV(ModelNode parent, string name, uint options, uint controlOptions, uint wpOptions, bool hasChaManRs)
			{
				IntPtr self = CDC_SPV_create(name, parent.self, options, controlOptions, wpOptions, hasChaManRs);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_STV(ModelNode parent, string name, uint options, uint controlOptions, uint wpOptions, bool hasOldStatus)
			{
				IntPtr self = CDC_STV_create(name, parent.self, options, controlOptions, wpOptions, hasOldStatus);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_CMD(ModelNode parent, string name, uint options, uint controlOptions, uint wpOptions, bool hasOldStatus, bool hasCmTm, bool hasCmCt)
			{
				IntPtr self = CDC_CMD_create(name, parent.self, options, controlOptions, wpOptions, hasOldStatus, hasCmTm, hasCmCt);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_ALM(ModelNode parent, string name, uint options, uint controlOptions, uint wpOptions, bool hasOldStatus)
			{
				IntPtr self = CDC_ALM_create(name, parent.self, options, controlOptions, wpOptions, hasOldStatus);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_CTE(ModelNode parent, string name, uint options, uint controlOptions, uint wpOptions, bool hasHisRs)
			{
				IntPtr self = CDC_CTE_create(name, parent.self, options, controlOptions, wpOptions, hasHisRs);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_TMS(ModelNode parent, string name, uint options, uint controlOptions, uint wpOptions, bool hasHisRs)
			{
				IntPtr self = CDC_TMS_create(name, parent.self, options, controlOptions, wpOptions, hasHisRs);

				if (self != IntPtr.Zero)
					return new DataObject(self);
				else
					return null;
			}

			public static DataObject Create_CDC_SPG_SG(ModelNode parent, string name, uint options)
			{
				DataObject newSPG = new DataObject(name, parent);
				new DataAttribute("setVal", newSPG, DataAttributeType.BOOLEAN, FunctionalConstraint.SG, TriggerOptions.DATA_CHANGED, 0, 0);
				new DataAttribute("setVal", newSPG, DataAttributeType.BOOLEAN, FunctionalConstraint.SE, TriggerOptions.DATA_CHANGED, 0, 0);
				new DataAttribute("d", newSPG, DataAttributeType.VISIBLE_STRING_255, FunctionalConstraint.DC, 0, 0, 0);
				new DataAttribute("dU", newSPG, DataAttributeType.UNICODE_STRING_255, FunctionalConstraint.DC, 0, 0, 0);
				new DataAttribute("dataNs", newSPG, DataAttributeType.VISIBLE_STRING_255, FunctionalConstraint.EX, 0, 0, 0);
				return newSPG;
			}

			public static DataObject Create_CDC_ING_SG(ModelNode parent, string name, uint options)
			{
				DataObject newING = new DataObject(name, parent);
				new DataAttribute("setVal", newING, DataAttributeType.INT32, FunctionalConstraint.SG, TriggerOptions.DATA_CHANGED, 0, 0);
				new DataAttribute("setVal", newING, DataAttributeType.INT32, FunctionalConstraint.SE, TriggerOptions.DATA_CHANGED, 0, 0);
				new DataAttribute("minVal", newING, DataAttributeType.INT32, FunctionalConstraint.CF, TriggerOptions.DATA_CHANGED, 0, 0);
				new DataAttribute("maxVal", newING, DataAttributeType.INT32, FunctionalConstraint.CF, TriggerOptions.DATA_CHANGED, 0, 0);
				new DataAttribute("stepSize", newING, DataAttributeType.INT32U, FunctionalConstraint.CF, TriggerOptions.DATA_CHANGED, 0, 0);
				new DataAttribute("d", newING, DataAttributeType.VISIBLE_STRING_255, FunctionalConstraint.DC, 0, 0, 0);
				new DataAttribute("dU", newING, DataAttributeType.UNICODE_STRING_255, FunctionalConstraint.DC, 0, 0, 0);
				new DataAttribute("dataNs", newING, DataAttributeType.VISIBLE_STRING_255, FunctionalConstraint.EX, 0, 0, 0);
				return newING;
			}

		}

		public class DataObject : ModelNode
		{
			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr DataObject_create(string name, IntPtr parent, int arrayElements);

			internal DataObject(IntPtr self) : base(self)
			{
			}

			public DataObject(string name, ModelNode parent) : this(name, parent, 0)
			{
			}

			public DataObject(string name, ModelNode parent, int arrayElements)
			{
				self = DataObject_create (name, parent.self, arrayElements);
			}

		}

		public class DataAttribute : ModelNode
		{

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr DataAttribute_create(string name, IntPtr parent, int type, int fc,
				byte triggerOptions, int arrayElements, UInt32 sAddr);

			internal DataAttribute(IntPtr self) : base(self)
			{
			}

			public DataAttribute (string name, ModelNode parent, DataAttributeType type, FunctionalConstraint fc, TriggerOptions trgOps,
				int arrayElements, UInt32 sAddr)
			{
				self = DataAttribute_create (name, parent.self, (int)type, (int)fc, (byte)trgOps, arrayElements, sAddr);
			}

		}

		public class ModelNode
		{
			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr ModelNode_getChild(IntPtr self, string name);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern int ModelNode_getType(IntPtr self);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr ModelNode_getChildWithFc(IntPtr self, string name, FunctionalConstraint fc);

			public IntPtr self;

			internal ModelNode()
			{
			}

			public ModelNode(IntPtr self)
			{
				this.self = self;
			}

			public ModelNode GetChild(string name)
			{
				IntPtr childPtr = ModelNode_getChild(self, name);

				if (childPtr == IntPtr.Zero)
					return null;

				int nodeType = ModelNode_getType (childPtr);

				switch (nodeType) {
				case 0:
					return new LogicalDevice (childPtr);

				case 1:
					return new LogicalNode (childPtr);

				case 2:
					return new DataObject (childPtr);

				case 3:
					return new DataAttribute (childPtr);

				default:
					return new ModelNode (childPtr);
				}

			}

			public ModelNode GetChildWithFc(string name, FunctionalConstraint fc)
			{
				IntPtr childPtr = ModelNode_getChildWithFc(self, name, fc);

				if (childPtr == IntPtr.Zero)
					return null;

				int nodeType = ModelNode_getType(childPtr);

				switch (nodeType)
				{
					case 0:
						return new LogicalDevice(childPtr);

					case 1:
						return new LogicalNode(childPtr);

					case 2:
						return new DataObject(childPtr);

					case 3:
						return new DataAttribute(childPtr);

					default:
						return new ModelNode(childPtr);
				}

			}
		}



		public class DataSet
		{
			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr DataSet_create(string name, IntPtr parent);

			public IntPtr self = IntPtr.Zero;

			public DataSet(string name, LogicalNode parent)
			{
				self = DataSet_create(name, parent.self);
			}
		}

		public class DataSetEntry
		{
			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr DataSetEntry_create(IntPtr dataSet, string variable, int index, string component);

			public IntPtr self = IntPtr.Zero;

			public DataSetEntry(DataSet dataSet, string variable, int index, string component)
			{
				self = DataSetEntry_create(dataSet.self, variable, index, component);
			}
		}

		public class ReportControlBlock
		{
			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr ReportControlBlock_create(string name, IntPtr parent, string rptId, [MarshalAs(UnmanagedType.I1)] bool isBuffered,
				string dataSetName, uint confRef, byte trgOps, byte options, uint bufTm, uint intgPd);

			public IntPtr self = IntPtr.Zero;

			public ReportControlBlock(string name, LogicalNode parent, string rptId, bool isBuffered,
				string dataSetName, uint confRev, byte trgOps, byte options, uint bufTm, uint intgPd)
			{
				self = ReportControlBlock_create(name, parent.self, rptId, isBuffered, dataSetName, confRev, trgOps, options, bufTm, intgPd);
			}
		}

		public class SettingGroupControlBlock
		{
			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr SettingGroupControlBlock_create(IntPtr parent, uint actSG, uint numOfSGs);

			public IntPtr self = IntPtr.Zero;

			public SettingGroupControlBlock(LogicalNode parent, uint actSG, uint numOfSGs)
			{
				self = SettingGroupControlBlock_create(parent.self, actSG, numOfSGs);
			}

			public SettingGroupControlBlock(IntPtr self)
			{
				this.self = self;
			}

		}

		public class ClientConnection 
		{
			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr ClientConnection_getPeerAddress(IntPtr self);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr ClientConnection_getLocalAddress(IntPtr self);

			internal IntPtr self;

			internal ClientConnection (IntPtr self) {
				this.self = self;
			}

			public string GetPeerAddress()
			{
				IntPtr peerAddrPtr = ClientConnection_getPeerAddress (self);

				if (peerAddrPtr != IntPtr.Zero)
					return Marshal.PtrToStringAnsi (peerAddrPtr);
				else
					return null;
			}

			public string GetLocalAddress()
			{
				IntPtr localAddrPtr = ClientConnection_getLocalAddress(self);

				if (localAddrPtr != IntPtr.Zero)
					return Marshal.PtrToStringAnsi(localAddrPtr);
				else
					return null;
			}
		}

        /// <summary>
        /// Represents additional context information of the control action that caused the callback invokation
        /// </summary>
        public class ControlAction
        {
            [DllImport ("iec61850", CallingConvention = CallingConvention.Cdecl)]
            static extern void ControlAction_setAddCause (IntPtr self, int addCause);

            [DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
            static extern void ControlAction_setError(IntPtr self, int error);

            [DllImport ("iec61850", CallingConvention = CallingConvention.Cdecl)]
            static extern int ControlAction_getOrCat (IntPtr self);

            [DllImport ("iec61850", CallingConvention = CallingConvention.Cdecl)]
            static extern IntPtr ControlAction_getOrIdent (IntPtr self, ref int size);

            [DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
            static extern IntPtr ControlAction_getClientConnection(IntPtr self);

            [DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
            static extern int ControlAction_getCtlNum(IntPtr self);

            [DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
            static extern UInt64 ControlAction_getControlTime(IntPtr self);

            [DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            static extern bool ControlAction_isSelect(IntPtr self);

            private IntPtr self;
            private IedServer.ControlHandlerInfo info;
            private IedServer iedServer;

            internal ControlAction (IntPtr self, IedServer.ControlHandlerInfo info, IedServer iedServer)
            {
                this.self = self;
                this.info = info;
                this.iedServer = iedServer;
            }

            /// <summary>
            /// Sets the error code for the next command termination or application error message.
            /// </summary>
            /// <param name="error">the errror code to use</param>
            public void SetError(ControlLastApplError error)
            {
                ControlAction_setError(self, (int)error);
            }

            /// <summary>
            /// Sets the add cause for the next command termination or application error message
            /// </summary>
            /// <param name="addCause">the additional cause code</param>
            public void SetAddCause (ControlAddCause addCause)
            {
                ControlAction_setAddCause (self, (int)addCause);
            }

            /// <summary>
            /// Gets the originator category provided by the client
            /// </summary>
            /// <returns>The or cat.</returns>
            public OrCat GetOrCat ()
            {
                return (OrCat)ControlAction_getOrCat (self);
            }

            /// <summary>
            ///  Get the originator identifier provided by the client
            /// </summary>
            /// <returns>The or ident.</returns>
            public byte [] GetOrIdent ()
            {
                int size = 0;

                IntPtr orIdentPtr = ControlAction_getOrIdent (self, ref size);

                if (orIdentPtr == IntPtr.Zero)
                    return null;

                byte [] orIdent = new byte [size];

                Marshal.Copy (orIdentPtr, orIdent, 0, size);

                return orIdent;
            }

            /// <summary>
            /// Gets the ctlNum attribute of the control action
            /// </summary>
            /// <returns>The ctlNum value. Valid values are restricted from 0 to 255, -1 means not present</returns>
            public int GetCtlNum()
            {
                return ControlAction_getCtlNum(self);
            }

            /// <summary>
            /// Gets the control object that is subject to this action
            /// </summary>
            /// <returns>the controllable data object instance</returns>
            public DataObject GetControlObject ()
            {
                return info.controlObject;
            }

            /// <summary>
            ///  Gets the time of control execution, if it's a time activated control
            /// </summary>
            /// <returns>The time of control execution or 0 for immediate execution</returns>
            public UInt64 GetControlTime()
            {
                return ControlAction_getControlTime(self);
            }

            /// <summary>
            /// Gets the tome of control execution as data time offset.
            /// </summary>
            /// <returns>The control execution time as data time offset.</returns>
            public DateTimeOffset GetControlTimeAsDataTimeOffset()
            {
                return MmsValue.MsTimeToDateTimeOffset(GetControlTime());
            }

            /// <summary>
            /// Gets the client object associated with the client that caused the control action
            /// </summary>
            /// <returns>The client connection.</returns>
            public ClientConnection GetClientConnection ()
            {
                ClientConnection con = null;

                IntPtr conPtr = ControlAction_getClientConnection (self);

                if (conPtr != IntPtr.Zero) {
                    iedServer.clientConnections.TryGetValue (conPtr, out con);
                }

                return con;
            }

            /// <summary>
            /// Cehck if the control callback is called by a select or operate command
            /// </summary>
            /// <returns><c>true</c>, if select, <c>false</c> otherwise.</returns>
            public bool IsSelect()
            {
                return ControlAction_isSelect(self);
            }
        }

		public delegate MmsDataAccessError WriteAccessHandler (DataAttribute dataAttr, MmsValue value, 
			ClientConnection connection, object parameter);

		public enum ControlHandlerResult {
			/// <summary>
			/// check or operation failed
			/// </summary>
			FAILED = 0,
			/// <summary>
			/// check or operation was successful
			/// </summary>
			OK = 1,
			/// <summary>
			/// check or operation is in progress
			/// </summary>
			WAITING = 2
		}

		public delegate ControlHandlerResult ControlWaitForExecutionHandler (ControlAction action, object parameter, MmsValue ctlVal, bool test, bool synchroCheck);

		public delegate ControlHandlerResult ControlHandler (ControlAction action, object parameter, MmsValue ctlVal, bool test);

		public enum CheckHandlerResult {
			/// <summary>
			/// check passed
			/// </summary>
			ACCEPTED = -1,
			/// <summary>
			/// check failed due to hardware fault
			/// </summary>
			HARDWARE_FAULT = 1,
			/// <summary>
			/// control is already selected or operated
			/// </summary>
			TEMPORARILY_UNAVAILABLE = 2,
			/// <summary>
			/// check failed due to access control reason - access denied for this client or state
			/// </summary>
			OBJECT_ACCESS_DENIED = 3,
			/// <summary>
			/// object not visible in this security context ??? 
			/// </summary>
			OBJECT_UNDEFINED = 4
		}

		public delegate CheckHandlerResult CheckHandler (ControlAction action, object parameter, MmsValue ctlVal, bool test, bool interlockCheck);

		public enum MmsError
		{
			/* generic error codes */
			MMS_ERROR_NONE = 0,
			MMS_ERROR_CONNECTION_REJECTED = 1,
			MMS_ERROR_CONNECTION_LOST = 2,
			MMS_ERROR_SERVICE_TIMEOUT = 3,
			MMS_ERROR_PARSING_RESPONSE = 4,
			MMS_ERROR_HARDWARE_FAULT = 5,
			MMS_ERROR_CONCLUDE_REJECTED = 6,
			MMS_ERROR_INVALID_ARGUMENTS = 7,
			MMS_ERROR_OUTSTANDING_CALL_LIMIT = 8,

			MMS_ERROR_OTHER = 9,

			/* confirmed error PDU codes */
			MMS_ERROR_VMDSTATE_OTHER = 10,

			MMS_ERROR_APPLICATION_REFERENCE_OTHER = 20,

			MMS_ERROR_DEFINITION_OTHER = 30,
			MMS_ERROR_DEFINITION_INVALID_ADDRESS = 31,
			MMS_ERROR_DEFINITION_TYPE_UNSUPPORTED = 32,
			MMS_ERROR_DEFINITION_TYPE_INCONSISTENT = 33,
			MMS_ERROR_DEFINITION_OBJECT_UNDEFINED = 34,
			MMS_ERROR_DEFINITION_OBJECT_EXISTS = 35,
			MMS_ERROR_DEFINITION_OBJECT_ATTRIBUTE_INCONSISTENT = 36,

			MMS_ERROR_RESOURCE_OTHER = 40,
			MMS_ERROR_RESOURCE_CAPABILITY_UNAVAILABLE = 41,

			MMS_ERROR_SERVICE_OTHER = 50,
			MMS_ERROR_SERVICE_OBJECT_CONSTRAINT_CONFLICT = 55,

			MMS_ERROR_SERVICE_PREEMPT_OTHER = 60,

			MMS_ERROR_TIME_RESOLUTION_OTHER = 70,

			MMS_ERROR_ACCESS_OTHER = 80,
			MMS_ERROR_ACCESS_OBJECT_NON_EXISTENT = 81,
			MMS_ERROR_ACCESS_OBJECT_ACCESS_UNSUPPORTED = 82,
			MMS_ERROR_ACCESS_OBJECT_ACCESS_DENIED = 83,
			MMS_ERROR_ACCESS_OBJECT_INVALIDATED = 84,
			MMS_ERROR_ACCESS_OBJECT_VALUE_INVALID = 85, /* for DataAccessError 11 */
			MMS_ERROR_ACCESS_TEMPORARILY_UNAVAILABLE = 86, /* for DataAccessError 2 */

			MMS_ERROR_FILE_OTHER = 90,
			MMS_ERROR_FILE_FILENAME_AMBIGUOUS = 91,
			MMS_ERROR_FILE_FILE_BUSY = 92,
			MMS_ERROR_FILE_FILENAME_SYNTAX_ERROR = 93,
			MMS_ERROR_FILE_CONTENT_TYPE_INVALID = 94,
			MMS_ERROR_FILE_POSITION_INVALID = 95,
			MMS_ERROR_FILE_FILE_ACCESS_DENIED = 96,
			MMS_ERROR_FILE_FILE_NON_EXISTENT = 97,
			MMS_ERROR_FILE_DUPLICATE_FILENAME = 98,
			MMS_ERROR_FILE_INSUFFICIENT_SPACE_IN_FILESTORE = 99,

			/* reject codes */
			MMS_ERROR_REJECT_OTHER = 100,
			MMS_ERROR_REJECT_UNKNOWN_PDU_TYPE = 101,
			MMS_ERROR_REJECT_INVALID_PDU = 102,
			MMS_ERROR_REJECT_UNRECOGNIZED_SERVICE = 103,
			MMS_ERROR_REJECT_UNRECOGNIZED_MODIFIER = 104,
			MMS_ERROR_REJECT_REQUEST_INVALID_ARGUMENT = 105
		}

		public enum MmsFileServiceType
		{
			MMS_FILE_ACCESS_TYPE_READ_DIRECTORY,
			MMS_FILE_ACCESS_TYPE_OPEN,
			MMS_FILE_ACCESS_TYPE_OBTAIN,
			MMS_FILE_ACCESS_TYPE_DELETE,
			MMS_FILE_ACCESS_TYPE_RENAME
		}

		public delegate MmsError FileAccessHandler(object parameter, ClientConnection clientConnection, MmsFileServiceType service, string localFilename, string otherFilename);

		public delegate bool ActiveSettingGroupChangedHandler(object parameter, SettingGroupControlBlock sgcb, uint newActSg, ClientConnection clientConnection);
		public delegate bool EditSettingGroupChangedHandler(object parameter, SettingGroupControlBlock sgcb, uint newEditSg, ClientConnection clientConnection);
		public delegate void EditSettingGroupConfirmationHandler(object parameter, SettingGroupControlBlock sgcb, uint editSg);

		/// <summary>
		/// This class acts as the entry point for the IEC 61850 client API. It represents a single
		/// (MMS) connection to a server.
		/// </summary>
		public class IedServer
		{
			[DllImport ("iec61850", CallingConvention=CallingConvention.Cdecl)]
			static extern IntPtr IedServer_createWithConfig(IntPtr modelRef, IntPtr tlsConfiguration, IntPtr serverConfiguratio);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_setLocalIpAddress(IntPtr self, string localIpAddress);

			[DllImport ("iec61850", CallingConvention=CallingConvention.Cdecl)]
			static extern void IedServer_start(IntPtr self, int tcpPort);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_stop(IntPtr self);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_destroy(IntPtr self);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.Bool)]
			static extern bool IedServer_isRunning(IntPtr self);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern int IedServer_getNumberOfOpenConnections(IntPtr self);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_lockDataModel(IntPtr self);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_unlockDataModel(IntPtr self);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_updateAttributeValue(IntPtr self, IntPtr DataAttribute, IntPtr MmsValue);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_updateBooleanAttributeValue(IntPtr self, IntPtr dataAttribute, [MarshalAs(UnmanagedType.I1)] bool value);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_updateInt32AttributeValue(IntPtr self, IntPtr dataAttribute, int value);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_updateInt64AttributeValue(IntPtr self, IntPtr dataAttribute, Int64 value);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_updateFloatAttributeValue(IntPtr self, IntPtr dataAttribute, float value);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_updateVisibleStringAttributeValue(IntPtr self, IntPtr dataAttribute, string value);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_updateUTCTimeAttributeValue(IntPtr self, IntPtr dataAttribute, ulong value);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_updateTimestampAttributeValue(IntPtr self, IntPtr dataAttribute, IntPtr timestamp);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_updateQuality(IntPtr self, IntPtr dataAttribute, ushort value);

            [DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
            static extern void IedServer_setServerIdentity(IntPtr self, string vendor, string model, string revision);

            [DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr IedServer_getAttributeValue(IntPtr self, IntPtr dataAttribute);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr IedServer_getMmsServer(IntPtr self);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr MmsServer_setFilestoreBasepath(IntPtr selfMmsServer, string basepath);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern IntPtr MmsServerConnection_getFilesystemBasepath(IntPtr selfMmsServer);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			private delegate int InternalControlPerformCheckHandler (IntPtr action, IntPtr parameter, IntPtr ctlVal, [MarshalAs(UnmanagedType.I1)] bool test, [MarshalAs(UnmanagedType.I1)] bool interlockCheck);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			private delegate int InternalControlWaitForExecutionHandler (IntPtr action, IntPtr parameter, IntPtr ctlVal, [MarshalAs(UnmanagedType.I1)] bool test, [MarshalAs(UnmanagedType.I1)] bool synchoCheck);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			private delegate int InternalControlHandler (IntPtr action, IntPtr parameter, IntPtr ctlVal, [MarshalAs(UnmanagedType.I1)] bool test);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_setWaitForExecutionHandler(IntPtr self, IntPtr node, InternalControlWaitForExecutionHandler handler, IntPtr parameter);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_setPerformCheckHandler(IntPtr self, IntPtr node, InternalControlPerformCheckHandler handler, IntPtr parameter);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_setControlHandler (IntPtr self, IntPtr node, InternalControlHandler handler, IntPtr parameter);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_setWriteAccessPolicy(IntPtr self, FunctionalConstraint fc, AccessPolicy policy);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			private delegate int InternalWriteAccessHandler (IntPtr dataAttribute, IntPtr value, IntPtr connection, IntPtr parameter);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_handleWriteAccess(IntPtr self, IntPtr dataAttribute,
				InternalWriteAccessHandler handler, IntPtr parameter);


            #region 定值组相关回调处理


            #region 激活定值组

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate int InternalActiveSettingGroupChangedHandler(IntPtr parameter, IntPtr sgcb, uint newActSg, IntPtr connection);

			[DllImport("iec61850", CallingConvention = CallingConvention.Winapi)]
			static extern void IedServer_setActiveSettingGroupChangedHandler(IntPtr self, IntPtr sgcb, InternalActiveSettingGroupChangedHandler handler, IntPtr parameter);

			private InternalActiveSettingGroupChangedHandler internalActiveSettingGroupChangedHandlerRef = null;

			private ActiveSettingGroupChangedHandler activeSettingGroupChangedHandler = null;
			private object activeSettingGroupChangedHandlerParameter = null;

			int internalActiveSettingGroupChangedHandler(IntPtr parameter, IntPtr sgcb, uint newActSg, IntPtr connection)
			{
				ClientConnection con = null;

				clientConnections.TryGetValue(connection, out con);

				var result = activeSettingGroupChangedHandler(activeSettingGroupChangedHandlerParameter, new SettingGroupControlBlock(sgcb), newActSg, con);

				return result ? 1 : 0;
			}

			public void SetActiveSettingGroupChangedHandler(SettingGroupControlBlock sgcb, ActiveSettingGroupChangedHandler handler, object parameter)
			{
				activeSettingGroupChangedHandler = handler;
				activeSettingGroupChangedHandlerParameter = parameter;

				if (internalActiveSettingGroupChangedHandlerRef == null)
					internalActiveSettingGroupChangedHandlerRef = new InternalActiveSettingGroupChangedHandler(internalActiveSettingGroupChangedHandler);

				IedServer_setActiveSettingGroupChangedHandler(self, sgcb.self, internalActiveSettingGroupChangedHandlerRef, IntPtr.Zero);
			}

            #endregion

            #region 编辑定值组

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate int InternalEditSettingGroupChangedHandler(IntPtr parameter, IntPtr sgcb, uint newEditSg, IntPtr connection);

			[DllImport("iec61850", CallingConvention = CallingConvention.Winapi)]
			static extern void IedServer_setEditSettingGroupChangedHandler(IntPtr self, IntPtr sgcb, InternalEditSettingGroupChangedHandler handler, IntPtr parameter);

			private InternalEditSettingGroupChangedHandler internalEditSettingGroupChangedHandlerRef = null;

			private EditSettingGroupChangedHandler editSettingGroupChangedHandler = null;
			private object editSettingGroupChangedHandlerParameter = null;

			int internalEditSettingGroupChangedHandler(IntPtr parameter, IntPtr sgcb, uint newEditSg, IntPtr connection)
			{
				ClientConnection con = null;

				clientConnections.TryGetValue(connection, out con);

				var result = editSettingGroupChangedHandler(editSettingGroupChangedHandlerParameter, new SettingGroupControlBlock(sgcb), newEditSg, con);

				return result ? 1 : 0;
			}

			public void SetEditSettingGroupChangedHandler(SettingGroupControlBlock sgcb, EditSettingGroupChangedHandler handler, object parameter)
			{
				editSettingGroupChangedHandler = handler;
				editSettingGroupChangedHandlerParameter = parameter;

				if (internalEditSettingGroupChangedHandlerRef == null)
					internalEditSettingGroupChangedHandlerRef = new InternalEditSettingGroupChangedHandler(internalEditSettingGroupChangedHandler);

				IedServer_setEditSettingGroupChangedHandler(self, sgcb.self, internalEditSettingGroupChangedHandlerRef, IntPtr.Zero);
			}

			#endregion

			#region 确认定值组

			//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			private delegate void InternalEditSettingGroupConfirmationHandler(IntPtr parameter, IntPtr sgcb, uint editSg);

			[DllImport("iec61850", CallingConvention = CallingConvention.Winapi)]
			static extern void IedServer_setEditSettingGroupConfirmationHandler(IntPtr self, IntPtr sgcb, InternalEditSettingGroupConfirmationHandler handler, IntPtr parameter);

			private InternalEditSettingGroupConfirmationHandler internalEditSettingGroupConfirmationHandlerRef = null;

			private EditSettingGroupConfirmationHandler editSettingGroupConfirmationHandler = null;
			private object editSettingGroupConfirmationHandlerParameter = null;

			void internalEditSettingGroupConfirmationHandler(IntPtr parameter, IntPtr sgcb, uint editSg)
			{
				editSettingGroupConfirmationHandler(editSettingGroupConfirmationHandlerParameter, new SettingGroupControlBlock(sgcb), editSg);
			}

			public void SetEditSettingGroupConfirmationHandler(SettingGroupControlBlock sgcb, EditSettingGroupConfirmationHandler handler, object parameter)
			{
				editSettingGroupConfirmationHandler = handler;
				editSettingGroupConfirmationHandlerParameter = parameter;

				if (internalEditSettingGroupConfirmationHandlerRef == null)
					internalEditSettingGroupConfirmationHandlerRef = new InternalEditSettingGroupConfirmationHandler(internalEditSettingGroupConfirmationHandler);

				IedServer_setEditSettingGroupConfirmationHandler(self, sgcb.self, internalEditSettingGroupConfirmationHandlerRef, IntPtr.Zero);
			}

			#endregion

			#endregion

			#region 文件服务相关回调处理

			//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			private delegate int InternalMmsFileAccessHandler(IntPtr parameter, IntPtr connection, int service, string localFilename, string otherFilename);

			[DllImport("iec61850", CallingConvention = CallingConvention.Winapi)]
			static extern void MmsServer_installFileAccessHandler(IntPtr self, InternalMmsFileAccessHandler handler, IntPtr parameter);

			private InternalMmsFileAccessHandler internalMmsFileAccessHandler = null;

			private FileAccessHandler mmsFileAccessHandler = null;
			private object mmsFileAccessHandlerParameter = null;

			int fileAccessHandler(IntPtr parameter, IntPtr connection, int service, string localFilename, string otherFilename)
			{
				ClientConnection con = null;

				clientConnections.TryGetValue(connection, out con);

				return (int)mmsFileAccessHandler(mmsFileAccessHandlerParameter, con, (MmsFileServiceType)service, localFilename, otherFilename);
			}

			public void SetFileAccessHandler(FileAccessHandler handler, object parameter)
			{
				mmsFileAccessHandler = handler;
				mmsFileAccessHandlerParameter = parameter;

				if (internalMmsFileAccessHandler == null)
					internalMmsFileAccessHandler = new InternalMmsFileAccessHandler(fileAccessHandler);

				IntPtr mmsServer = IedServer_getMmsServer(self);

				MmsServer_installFileAccessHandler(mmsServer, internalMmsFileAccessHandler, IntPtr.Zero);
			}

			#endregion


			public delegate void ConnectionIndicationHandler(IedServer iedServer, ClientConnection clientConnection, bool connected, object parameter);

			private ConnectionIndicationHandler connectionHandler = null;
			private object connectionHandlerParameter = null;

			public void SetConnectionIndicationHandler(ConnectionIndicationHandler handler, object parameter)
			{
				connectionHandler = handler;
				connectionHandlerParameter = parameter;
			}

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			private delegate void InternalConnectionHandler (IntPtr iedServer, IntPtr clientConnection, [MarshalAs(UnmanagedType.I1)] bool connected, IntPtr parameter);

			[DllImport("iec61850", CallingConvention = CallingConvention.Cdecl)]
			static extern void IedServer_setConnectionIndicationHandler(IntPtr self, InternalConnectionHandler handler, IntPtr parameter);

			private IntPtr self = IntPtr.Zero;

			private InternalControlHandler internalControlHandlerRef = null;
			private InternalControlPerformCheckHandler internalControlPerformCheckHandlerRef = null;
			private InternalControlWaitForExecutionHandler internalControlWaitForExecutionHandlerRef = null;
			private InternalWriteAccessHandler internalWriteAccessHandlerRef = null;

			internal class ControlHandlerInfo {
				public DataObject controlObject = null;
				public GCHandle handle;

				public ControlHandler controlHandler = null;
				public object controlHandlerParameter = null;

				public CheckHandler checkHandler = null;
				public object checkHandlerParameter = null;

				public ControlWaitForExecutionHandler waitForExecHandler = null;
				public object waitForExecHandlerParameter = null;

				public ControlHandlerInfo(DataObject controlObject)
				{
					this.controlObject = controlObject;
					this.handle = GCHandle.Alloc(this);
				}
				~ControlHandlerInfo() {
					this.handle.Free();
				}
					
			}

			private Dictionary<DataObject, ControlHandlerInfo> controlHandlers = new Dictionary<DataObject, ControlHandlerInfo> ();

			int InternalControlHandlerImpl (IntPtr action, IntPtr parameter, IntPtr ctlVal, bool test)
			{
				GCHandle handle = GCHandle.FromIntPtr (parameter);

				ControlHandlerInfo info = (ControlHandlerInfo)handle.Target;

                ControlAction controlAction = new ControlAction (action, info, this);

				if (info != null & info.controlHandler != null)
					return (int)info.controlHandler (controlAction, info.controlHandlerParameter, new MmsValue (ctlVal), test);
				else
					return (int)ControlHandlerResult.FAILED;
			}

			int InternalCheckHandlerImpl(IntPtr action, IntPtr parameter, IntPtr ctlVal, bool test, bool interlockCheck)
			{
				GCHandle handle = GCHandle.FromIntPtr (parameter);

				ControlHandlerInfo info = (ControlHandlerInfo)handle.Target;

				if (info != null & info.checkHandler != null) {

                    ControlAction controlAction = new ControlAction (action, info, this);

					return (int)info.checkHandler (controlAction, info.checkHandlerParameter, new MmsValue (ctlVal), test, interlockCheck); 
				} else
					return (int)CheckHandlerResult.OBJECT_UNDEFINED;
			}

			int InternalControlWaitForExecutionHandlerImpl (IntPtr action, IntPtr parameter, IntPtr ctlVal, bool test, bool synchoCheck)
			{
				GCHandle handle = GCHandle.FromIntPtr (parameter);

				ControlHandlerInfo info = (ControlHandlerInfo)handle.Target;

				if (info != null & info.waitForExecHandler != null) {

                    ControlAction controlAction = new ControlAction (action, info, this);

                    return (int)info.waitForExecHandler (controlAction, info.waitForExecHandlerParameter, new MmsValue (ctlVal), test, synchoCheck);
				} 
				else
					return (int)ControlHandlerResult.FAILED;
			}

			private struct WriteAccessHandlerInfo {
				public WriteAccessHandler handler;
				public object parameter;
				public DataAttribute dataAttribute;

				public WriteAccessHandlerInfo (WriteAccessHandler h, object p, DataAttribute da) 
				{
					handler = h;
					parameter = p;
					dataAttribute = da;
				}
			}

			int WriteAccessHandlerImpl (IntPtr dataAttribute, IntPtr value, IntPtr connection, IntPtr parameter)
			{
				//object info = writeAccessHandlers.Item [dataAttribute];
				WriteAccessHandlerInfo info;

				writeAccessHandlers.TryGetValue (dataAttribute, out info);

				ClientConnection con = null;

				clientConnections.TryGetValue (connection, out con);

				return (int) info.handler (info.dataAttribute, new MmsValue (value), con, info.parameter);
			}

			private Dictionary<IntPtr, WriteAccessHandlerInfo> writeAccessHandlers = new Dictionary<IntPtr, WriteAccessHandlerInfo> ();

			private void ConnectionIndicationHandlerImpl (IntPtr iedServer, IntPtr clientConnection, bool connected, IntPtr parameter)
			{
				if (connected == false) {
					ClientConnection con = null;

					clientConnections.TryGetValue (clientConnection, out con);

					if (con != null) {
						
						if (connectionHandler != null)
							connectionHandler (this, con, false, connectionHandlerParameter);

						clientConnections.Remove (clientConnection);
					}
				} else {
					ClientConnection con = new ClientConnection (clientConnection);

					clientConnections.Add (clientConnection, con);

					if (connectionHandler != null)
						connectionHandler (this, con, true, connectionHandlerParameter);
				}
			}

			internal Dictionary<IntPtr, ClientConnection> clientConnections = new Dictionary<IntPtr, ClientConnection> ();

			public IedServer(IedModel iedModel, IedServerConfig config = null)
			{
				IntPtr nativeConfig = IntPtr.Zero;

				if (config != null)
					nativeConfig = config.self;

				self = IedServer_createWithConfig (iedModel.self, IntPtr.Zero, nativeConfig);
			}

			public IedServer(IedModel iedModel, TLSConfiguration tlsConfig, IedServerConfig config = null)
			{
				IntPtr nativeConfig = IntPtr.Zero;
				IntPtr nativeTLSConfig = IntPtr.Zero;

				if (config != null)
					nativeConfig = config.self;

				if (tlsConfig != null)
					nativeTLSConfig = tlsConfig.GetNativeInstance ();

				self = IedServer_createWithConfig (iedModel.self, nativeTLSConfig, nativeConfig);
			}

			// causes undefined behavior
			//~IedServer()
			//{
			//    if (self != IntPtr.Zero)
			//    {
			//        IedServer_destroy(self);
			//    }
			//}

			private InternalConnectionHandler internalConnectionHandler = null;

			/// <summary>
			/// Sets the local ip address for listening
			/// </summary>
			/// <param name="localIpAddress">Local IP address.</param>
			public void SetLocalIpAddress(string localIpAddress)
			{
				IedServer_setLocalIpAddress (self, localIpAddress);
			}

			/// <summary>
			/// Start MMS server
			/// </summary>
			/// <param name="localIpAddress">Local IP address.</param>
			/// <param name="tcpPort">TCP port to use</param>
			public void Start(string localIpAddress, int tcpPort)
			{
				SetLocalIpAddress (localIpAddress);
				Start (tcpPort);
			}

			/// <summary>Start MMS server</summary>
			/// <param name="tcpPort">TCP port to use</param>
			public void Start(int tcpPort)
			{
				if (internalConnectionHandler == null)
					internalConnectionHandler = new InternalConnectionHandler (ConnectionIndicationHandlerImpl);					

				IedServer_setConnectionIndicationHandler (self, internalConnectionHandler, IntPtr.Zero);

				IedServer_start(self, tcpPort);
			}

			/// <summary>Start MMS server</summary>
			public void Start ()
			{
				Start(-1);
			}

			/// <summary>
			/// Stop the MMS server.
			/// </summary>
			/// <description>This function will stop the server. This will close the TCP server socket and all client sockets.</description>
			public void Stop()
			{
				IedServer_stop(self);
				internalConnectionHandler = null;
			}

			/// <summary>
			/// Release all server resources.
			/// </summary>
			/// <description>This function releases all MMS server resources.</description>
			public void Destroy()
			{
				IedServer_destroy(self);
				self = IntPtr.Zero;
				internalConnectionHandler = null;
			}

            /// <summary>
            /// Set the identify for the MMS identify service
            /// </summary>
            /// <param name="vendor">the IED vendor name</param>
            /// <param name="model">the IED model name</param>
            /// <param name="revision">the IED revision/version number</param>
            public void SetServerIdentity(string vendor, string model, string revision)
            {
                IedServer_setServerIdentity(self, vendor, model, revision);
            }

            /// <summary>
            /// Check if server is running (accepting client connections)
            /// </summary>
            /// <returns><c>true</c>, if running, <c>false</c> otherwise.</returns>
			public bool IsRunning()
            {
                return IedServer_isRunning(self);
            }

			/// <summary>
			/// Get number of open MMS connections
			/// </summary>
			/// <returns>the number of open and accepted MMS connections</returns>
			public int GetNumberOfOpenConnections()
			{
				return IedServer_getNumberOfOpenConnections(self);
			}

            private ControlHandlerInfo GetControlHandlerInfo(DataObject controlObject)
			{
				ControlHandlerInfo info;

				controlHandlers.TryGetValue (controlObject, out info);

				if (info == null) {
					info = new ControlHandlerInfo (controlObject);
					controlHandlers.Add (controlObject, info);
				}

				return info;
			}

			public void SetControlHandler (DataObject controlObject, ControlHandler handler, object parameter)
			{
				ControlHandlerInfo info = GetControlHandlerInfo (controlObject);

				info.controlHandler = handler;
				info.controlHandlerParameter = parameter;

				if (internalControlHandlerRef == null)
					internalControlHandlerRef = new InternalControlHandler (InternalControlHandlerImpl);

				IedServer_setControlHandler(self, controlObject.self, internalControlHandlerRef, GCHandle.ToIntPtr(info.handle));
			}

			public void SetCheckHandler (DataObject controlObject, CheckHandler handler, object parameter)
			{
				ControlHandlerInfo info = GetControlHandlerInfo (controlObject);

				info.checkHandler = handler;
				info.checkHandlerParameter = parameter;

				if (internalControlPerformCheckHandlerRef == null)
					internalControlPerformCheckHandlerRef = new InternalControlPerformCheckHandler (InternalCheckHandlerImpl);

				IedServer_setPerformCheckHandler(self, controlObject.self, internalControlPerformCheckHandlerRef, GCHandle.ToIntPtr(info.handle));
			}

			public void SetWaitForExecutionHandler (DataObject controlObject, ControlWaitForExecutionHandler handler, object parameter)
			{
				ControlHandlerInfo info = GetControlHandlerInfo (controlObject);

				info.waitForExecHandler = handler;
				info.waitForExecHandlerParameter = parameter;

				if (internalControlWaitForExecutionHandlerRef == null)
					internalControlWaitForExecutionHandlerRef = new InternalControlWaitForExecutionHandler (InternalControlWaitForExecutionHandlerImpl);

				IedServer_setWaitForExecutionHandler(self, controlObject.self, internalControlWaitForExecutionHandlerRef, GCHandle.ToIntPtr(info.handle));
			}
				
			public void HandleWriteAccess (DataAttribute dataAttr, WriteAccessHandler handler, object parameter)
			{
				writeAccessHandlers.Add (dataAttr.self, new WriteAccessHandlerInfo(handler, parameter, dataAttr));
				//writeAccessHandlers.Item [dataAttr.self] = handler;
				if (internalWriteAccessHandlerRef == null)
					internalWriteAccessHandlerRef = new InternalWriteAccessHandler(WriteAccessHandlerImpl);
				IedServer_handleWriteAccess (self, dataAttr.self, internalWriteAccessHandlerRef, IntPtr.Zero);
			}

			public void SetWriteAccessPolicy(FunctionalConstraint fc, AccessPolicy policy)
			{
				IedServer_setWriteAccessPolicy (self, fc, policy);
			}

			public void SetFilestoreBasepath(string basepath)
			{
				IntPtr mmsServer = IedServer_getMmsServer(self);
				MmsServer_setFilestoreBasepath(mmsServer, basepath);
			}

			public void LockDataModel()
			{
				IedServer_lockDataModel(self);
			}

			public void UnlockDataModel()
			{
				IedServer_unlockDataModel(self);
			}

			public void UpdateAttributeValue(DataAttribute dataAttr, MmsValue value)
			{
				IedServer_updateAttributeValue (self, dataAttr.self, value.valueReference);
			}

			public void UpdateBooleanAttributeValue(DataAttribute dataAttr, bool value)
			{
				IedServer_updateBooleanAttributeValue(self, dataAttr.self, value);
			}

			public void UpdateFloatAttributeValue(DataAttribute dataAttr, float value)
			{
				IedServer_updateFloatAttributeValue(self, dataAttr.self, value);
			}

			public void UpdateInt32AttributeValue(DataAttribute dataAttr, int value)
			{
				IedServer_updateInt32AttributeValue(self, dataAttr.self, value);
			}

			public void UpdateInt64AttributeValue(DataAttribute dataAttr, Int64 value)
			{
				IedServer_updateInt64AttributeValue (self, dataAttr.self, value);
			}

			public void UpdateVisibleStringAttributeValue(DataAttribute dataAttr, string value)
			{
				IedServer_updateVisibleStringAttributeValue(self, dataAttr.self, value);
			}

			public void UpdateUTCTimeAttributeValue(DataAttribute dataAttr, DateTime timestamp)
			{
				DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				DateTime timestampUTC = timestamp.ToUniversalTime();

				TimeSpan timeDiff = timestampUTC - epoch;
				ulong timeVal = Convert.ToUInt64(timeDiff.TotalMilliseconds);

				IedServer_updateUTCTimeAttributeValue(self, dataAttr.self, timeVal);
			}

			public void UpdateTimestampAttributeValue(DataAttribute dataAttr, Timestamp timestamp)
			{
				IedServer_updateTimestampAttributeValue (self, dataAttr.self, timestamp.timestampRef);
			}

			public void UpdateQuality(DataAttribute dataAttr, ushort value)
			{
				IedServer_updateQuality(self, dataAttr.self, value);
			}

			public MmsValue GetAttributeValue(DataAttribute dataAttr)
			{
				IntPtr mmsValuePtr = IedServer_getAttributeValue (self, dataAttr.self);

				if (mmsValuePtr != IntPtr.Zero)
					return new MmsValue (mmsValuePtr);
				else
					return null;
			}
		}

	}
}
