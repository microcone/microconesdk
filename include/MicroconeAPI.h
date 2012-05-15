////////////////////////////////////////////////////////////////
//
// Copyright (c) 2012 Dev-Audio, All rights reserved.
//
// www.dev-audio.com
//
////////////////////////////////////////////////////////////////

#ifndef MicroconeAPI_h
#define MicroconeAPI_h

#ifdef _MSC_VER
#ifdef MICROCONEFRAMEWORK_EXPORTS
#define MICROCONEDLL_API __declspec(dllexport)
#else
#define MICROCONEDLL_API __declspec(dllimport)
#endif
#else
#define MICROCONEDLL_API  
#endif

#ifdef __cplusplus
extern "C" {
#endif
    
#ifdef _MSC_VER
    typedef __int32 SInt32;
#else
#define _stdcall
#endif
    
    /*!
     \brief This callback signature is used to be alerted of changes to the sector-based activity and location parameters.
	 
     \param sectorActivity Output vector of automated booleans (integer 1 is true, integer 0 false) per sector indicating current speech activity.  Length is kNumSectors.
     \param sectorLocation Output vector of automated azimuth location per sector, ranging from 0 (left boundary of sector when facing towards device) to 1 (right boundary of sector).  Length is kNumSectors.
     */
    MICROCONEDLL_API typedef void _stdcall MicroconeCallbackFunction(int* sectorActivity, float* sectorLocation);
    
    /*!
     \brief This indicates the number of logical sectors around the device.
     */
#define kNumSectors        6
    
    /*!
     \brief Error code indicating failure from the API functions.
     */
    enum 
    {
        kErrorInvalidClient = -1
    };
    
    /*!
     \brief This function creates a connection with the driver, and must be called prior to all other functions.  This should only be called once for each client application, ideally on application launch.
	 
     \param callback Function pointer conforming to the MicroconeCallbackFunction definition above.
     
     \returns Client ID to be used as input parameter for all other API functions.
     */
    MICROCONEDLL_API extern SInt32 InitClientConnection(MicroconeCallbackFunction* callback);
    
    
    /*!
     \brief This function closes the connection with the driver.  This should only be called once for each client application, ideally on application termination.
	 
     \param clientId Client ID returned by InitClientConnection().
     */
    MICROCONEDLL_API extern void CloseClientConnection(SInt32 clientId);
    
    /*!
     \brief Settor function for the Microcone processing stereo module state.
	 
     \param clientId Client ID returned by InitClientConnection().
     \param doStereo Desired state of the stereo module, as integer 0 for off or 1 for on.
     
     \returns Status code: 0 on success, else error codes defined above.
     */
    MICROCONEDLL_API extern SInt32 SetDoStereo(SInt32 clientId, int doStereo);
    
    /*!
     \brief Accessor function for the Microcone processing stereo module state.
	 
     \param clientId Client ID returned by InitClientConnection().
     \param doStereo On return, holds the current state of the stereo module, as integer 0 for off or 1 for on.
     
     \returns Status code: 0 on success, else error codes defined above.
     */
    MICROCONEDLL_API extern SInt32 GetDoStereo(SInt32 clientId, int* doStereo);
    
    
    /*!
     \brief Settor function for the Microcone processing sector gains to be used in producing the mixed signal output.
	 
     \param clientId Client ID returned by InitClientConnection().
     \param sectorGain Vector of desired gain levels for each sector. Length is kNumSectors
     
     \returns Status code: 0 on success, else error codes defined above.
     */
    MICROCONEDLL_API extern SInt32 SetGain(SInt32 clientId, float* sectorGain);
    
    /*!
     \brief Accessor function for the Microcone processing sector gains currently used in producing the mixed signal output.
	 
     \param clientId Client ID returned by InitClientConnection().
     \param sectorGain On return, vector of current gain levels for each sector. Length is kNumSectors
     
     \returns Status code: 0 on success, else error codes defined above.
     */
    MICROCONEDLL_API extern SInt32 GetGain(SInt32 clientId, float* sectorGain);
    
    /*!
     \brief Settor function for the Microcone processing sector enabled states to be used in producing the mixed signal output.
	 
     \param clientId Client ID returned by InitClientConnection().
     \param sectorEnabled Vector of desired enabled states for each sector, as integer 0 for off or 1 for on. Length is kNumSectors
     
     \returns Status code: 0 on success, else error codes defined above.
     */
    MICROCONEDLL_API extern SInt32 SetEnabled(SInt32 clientId, int* sectorEnabled);
    
    
    /*!
     \brief Accessor function for the Microcone processing sector enabled states currently used in producing the mixed signal output.
	 
     \param clientId Client ID returned by InitClientConnection().
     \param sectorEnabled On return, vector of current enabled states for each sector, as integer 0 for off or 1 for on. Length is kNumSectors
     
     \returns Status code: 0 on success, else error codes defined above.
     */
    MICROCONEDLL_API extern SInt32 GetEnabled(SInt32 clientId, int* sectorEnabled);
    
    /*!
     \brief Settor function for the Microcone DSP enabled state.
	 
     \param clientId Client ID returned by InitClientConnection().
     \param enabled Desired state of the DSP module, as integer 0 for off or 1 for on.  Setting to 0 disables all DSP, making the device simply pass through raw microphone audio, so should only be done in rare use cases.
     
     \returns Status code: 0 on success, else error codes defined above.
     */
    MICROCONEDLL_API extern SInt32 SetDspEnabled(SInt32 clientId, int enabled);
    
    
    /*!
     \brief Accessor function for the Microcone DSP enabled state.
	 
     \param clientId Client ID returned by InitClientConnection().
     \param enabled On return, holds current state of the DSP, as integer 0 for off or 1 for on.
     
     \returns Status code: 0 on success, else error codes defined above.
     */
    MICROCONEDLL_API extern SInt32 GetDspEnabled(SInt32 clientId, int* enabled);
    
#ifdef __cplusplus
} // (end extern "C")
#endif

#endif
