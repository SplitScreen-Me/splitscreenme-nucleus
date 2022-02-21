/* Copyright (C) Nemirtingas
 * This file is part of the OpenXinput project.
 *
 * OpenXinput project is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * OpenXinput project is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with OpenXinput project.  If not, see <https://www.gnu.org/licenses/>
 */

#pragma once

#ifdef OPENXINPUT_STATIC_GUIDS
 // Build guids, don't need to link against other dll
#define INITGUID
#else
#endif

#define VC_EXTRALEAN
#define NOMINMAX

#include "OpenXinput.h"
#include <strsafe.h>
#include <SetupAPI.h>

 // Direct Input is deprecated, copy here the minimal stuff needed
#ifdef OPENXINPUT_DISABLE_DSOUND_HEADERS

typedef enum
{
    DIRECTSOUNDDEVICE_TYPE_EMULATED,
    DIRECTSOUNDDEVICE_TYPE_VXD,
    DIRECTSOUNDDEVICE_TYPE_WDM
} DIRECTSOUNDDEVICE_TYPE;

typedef enum
{
    DIRECTSOUNDDEVICE_DATAFLOW_RENDER,
    DIRECTSOUNDDEVICE_DATAFLOW_CAPTURE
} DIRECTSOUNDDEVICE_DATAFLOW;

typedef struct _DSPROPERTY_DIRECTSOUNDDEVICE_DESCRIPTION_W_DATA
{
    DIRECTSOUNDDEVICE_TYPE      Type;           // Device type
    DIRECTSOUNDDEVICE_DATAFLOW  DataFlow;       // Device dataflow
    GUID                        DeviceId;       // DirectSound device id
    LPWSTR                      Description;    // Device description
    LPWSTR                      Module;         // Device driver module
    LPWSTR                      Interface;      // Device interface
    ULONG                       WaveDeviceId;   // Wave device id
} DSPROPERTY_DIRECTSOUNDDEVICE_DESCRIPTION_W_DATA, * PDSPROPERTY_DIRECTSOUNDDEVICE_DESCRIPTION_W_DATA;

typedef BOOL(CALLBACK* LPFNDIRECTSOUNDDEVICEENUMERATECALLBACKW)(PDSPROPERTY_DIRECTSOUNDDEVICE_DESCRIPTION_W_DATA, LPVOID);

typedef struct _DSPROPERTY_DIRECTSOUNDDEVICE_ENUMERATE_W_DATA
{
    LPFNDIRECTSOUNDDEVICEENUMERATECALLBACKW Callback;   // Callback function pointer
    LPVOID                                  Context;    // Callback function context argument
} DSPROPERTY_DIRECTSOUNDDEVICE_ENUMERATE_W_DATA, * PDSPROPERTY_DIRECTSOUNDDEVICE_ENUMERATE_W_DATA;


DECLARE_INTERFACE_(IKsPropertySet, IUnknown)
{
    // IUnknown methods
    STDMETHOD(QueryInterface)   (THIS_ _In_ REFIID, _Outptr_ LPVOID*) PURE;
    STDMETHOD_(ULONG, AddRef)    (THIS) PURE;
    STDMETHOD_(ULONG, Release)   (THIS) PURE;

    // IKsPropertySet methods
    STDMETHOD(Get)              (THIS_ _In_ REFGUID rguidPropSet, ULONG ulId, _In_reads_bytes_opt_(ulInstanceLength) LPVOID pInstanceData, ULONG ulInstanceLength,
                                 _Out_writes_bytes_(ulDataLength) LPVOID pPropertyData, ULONG ulDataLength, _Out_opt_ PULONG pulBytesReturned) PURE;
    STDMETHOD(Set)              (THIS_ _In_ REFGUID rguidPropSet, ULONG ulId, _In_reads_bytes_opt_(ulInstanceLength)  LPVOID pInstanceData, ULONG ulInstanceLength,
                                 _In_reads_bytes_(ulDataLength) LPVOID pPropertyData, ULONG ulDataLength) PURE;
    STDMETHOD(QuerySupport)     (THIS_ _In_ REFGUID rguidPropSet, ULONG ulId, _Out_ PULONG pulTypeSupport) PURE;
};

#else

#include <dsound.h>
#include <dsconf.h>

#endif

#define DEVICE_STATUS_ACTIVE 1

#define DEFINE_HIDDEN_GUID(name, l, w1, w2, b1, b2, b3, b4, b5, b6, b7, b8) \
        static const GUID name = { l, w1, w2, { b1, b2, b3, b4, b5, b6, b7, b8 } }

typedef HRESULT(WINAPI DllGetClassObject_t)(const GUID*, const GUID*, LPVOID*);
#ifdef OPENXINPUT_STATIC_GUIDS
DEFINE_HIDDEN_GUID(static_XINPUT_IID_IClassFactory, 0x00000001, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);
DEFINE_HIDDEN_GUID(static_XINPUT_IID_IKsPropertySet, 0x31efac30, 0x515c, 0x11d0, 0xa9, 0xaa, 0x00, 0xaa, 0x00, 0x61, 0xbe, 0x93);
DEFINE_HIDDEN_GUID(static_XINPUT_CLSID_DirectSoundPrivate, 0x11ab3ec0, 0x25ec, 0x11d1, 0xa4, 0xd8, 0x00, 0xc0, 0x4f, 0xc2, 0x8a, 0xca);
DEFINE_HIDDEN_GUID(static_XINPUT_DSPROPSETID_DirectSoundDevice, 0x84624f82, 0x25ec, 0x11d1, 0xa4, 0xd8, 0x00, 0xc0, 0x4f, 0xc2, 0x8a, 0xca);

#define XINPUT_IID_IClassFactory             static_XINPUT_IID_IClassFactory
#define XINPUT_IID_IKsPropertySet            static_XINPUT_IID_IKsPropertySet
#define XINPUT_CLSID_DirectSoundPrivate      static_XINPUT_CLSID_DirectSoundPrivate
#define XINPUT_DSPROPSETID_DirectSoundDevice static_XINPUT_DSPROPSETID_DirectSoundDevice
#else
#define XINPUT_IID_IClassFactory             IID_IClassFactory

#ifdef OPENXINPUT_DISABLE_DSOUND_HEADERS
DEFINE_HIDDEN_GUID(static_XINPUT_IID_IKsPropertySet, 0x31efac30, 0x515c, 0x11d0, 0xa9, 0xaa, 0x00, 0xaa, 0x00, 0x61, 0xbe, 0x93);
DEFINE_HIDDEN_GUID(static_XINPUT_CLSID_DirectSoundPrivate, 0x11ab3ec0, 0x25ec, 0x11d1, 0xa4, 0xd8, 0x00, 0xc0, 0x4f, 0xc2, 0x8a, 0xca);
DEFINE_HIDDEN_GUID(static_XINPUT_DSPROPSETID_DirectSoundDevice, 0x84624f82, 0x25ec, 0x11d1, 0xa4, 0xd8, 0x00, 0xc0, 0x4f, 0xc2, 0x8a, 0xca);

#define XINPUT_IID_IKsPropertySet            static_XINPUT_IID_IKsPropertySet
#define XINPUT_CLSID_DirectSoundPrivate      static_XINPUT_CLSID_DirectSoundPrivate
#define XINPUT_DSPROPSETID_DirectSoundDevice static_XINPUT_DSPROPSETID_DirectSoundDevice
#else
#define XINPUT_CLSID_DirectSoundPrivate      CLSID_DirectSoundPrivate
#define XINPUT_DSPROPSETID_DirectSoundDevice DSPROPSETID_DirectSoundDevice
#endif
#endif