//
//  MCAppDelegate.m
//  Microcone Demo
//
//  Copyright 2012 DEV-AUDIO Pty Ltd. All rights reserved.
//

#import "MCAppDelegate.h"
#import "MCUSBDetect.h"
#import <MicroconeAPI/MicroconeAPI.h>


MCSectorView * globalSectorView ;

void callback(int* sectorActivity, float* sectorLocation);
void callback(int* sectorActivity, float* sectorLocation)
{
    if ( globalSectorView != nil ) {
        [globalSectorView setActive:sectorActivity] ;
        [globalSectorView setAzimuth:sectorLocation] ;
        [globalSectorView setNeedsDisplay:YES] ;    
    }
}


@implementation MCAppDelegate

@synthesize window = _window;
@synthesize sectorView ;
@synthesize mMixVolume ;
@synthesize microconeConnected ;

- (void)applicationWillTerminate:(NSNotification *)notification
{
    CloseClientConnection(clientId);
    [mUpdateTimer invalidate] ;
    mUpdateTimer = nil ;
}


- (void)dealloc
{
    [super dealloc];
}

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification
{
    // Insert code here to initialize your application
    globalSectorView = sectorView ;
    
    [self resetInterface:self] ;
    
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(connected:) name:@"DAMicroconeConnected" object:nil];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(disconnected:) name:@"DAMicroconeDisconnected" object:nil];
    
    [self setMMixVolume:1.0] ;
    self.microconeConnected = [NSNumber numberWithBool:YES] ;
    [self resetInterface:self] ;
    clientId = InitClientConnection(callback);
	initUSBDetect() ;
    
    mUpdateTimer = [NSTimer scheduledTimerWithTimeInterval:30.0
                                                    target:self
                                                  selector:@selector(updateFromDriver:)
                                                  userInfo:nil
                                                   repeats:YES] ;
}

- (void)resetInterface:(id)sender
{
    self.microconeConnected = [NSNumber numberWithBool:YES] ;
    
    int enabled ;
    GetDoStereo(clientId, &enabled) ;
    if ( enabled == 0 ) {
		[stereoToolbarItem setImage:[NSImage imageNamed:@"stereoTemplate"]] ; 
    } else {
		[stereoToolbarItem setImage:[NSImage imageNamed:@"stereo"]] ; 
    }
    
	GetDspEnabled(clientId, &enabled) ;
    if ( enabled == 0 ) {
		[dspToolbarItem setImage:[NSImage imageNamed:@"gray-light"]] ; 
    } else {
		[dspToolbarItem setImage:[NSImage imageNamed:@"blue-light"]] ; 
    }
    
    GetGain( clientId, mCellVolumes) ;
    GetEnabled( clientId, mCellEnabled) ;
    for ( int i=0; i<6; i++ ) {
        float sliderValue ; 
        sliderValue = mCellVolumes[i] / [self logToLinear:mMixVolume] ;
        sliderValue = [self linearToLog:mCellVolumes[i]] ;
        [[mSliderMatrix cellWithTag:i] setFloatValue:sliderValue] ;
        
        if ( mCellEnabled[i] == 1 ) {
            [mButtonMatrix setSelected:YES forSegment:i] ;
        } else {
            [mButtonMatrix setSelected:NO forSegment:i] ;
        }
    }
	[mResetButton setEnabled:YES] ;
	[sectorView setEnabled:mCellEnabled] ;
	[sectorView setNeedsDisplay:YES] ;
}


- (void)connected:(NSNotification *)note
{
    [self performSelector:@selector(resetInterface:) withObject:nil afterDelay:1.0] ;
}

- (void)disconnected:(NSNotification *)note
{
    self.microconeConnected = [NSNumber numberWithBool:NO] ;
    [dspToolbarItem setImage:[NSImage imageNamed:@"gray-light"]] ; 
}


- (IBAction)onEnableDSP:(id)sender
{
    int dspEnabled ;
    
	GetDspEnabled(clientId, &dspEnabled) ;
    
    if ( dspEnabled == 0 ) {
        SetDspEnabled(clientId, 1) ;
		[dspToolbarItem setImage:[NSImage imageNamed:@"blue-light"]] ; 
    } else {
        NSInteger retVal = NSRunAlertPanel( @"Warning", @"This will turn off the Microcone signal processing. Only do this if you want to access the raw microphone signals.\n\nDo you wish to proceed?", @"No", @"Yes", nil ) ;
        if ( retVal == NSAlertAlternateReturn ) {
            SetDspEnabled(clientId, 0) ;
            [dspToolbarItem setImage:[NSImage imageNamed:@"gray-light"]] ; 
        }
    }
}

- (IBAction)onEnableStereo:(id)sender
{
    int stereoEnabled ;
    
    GetDoStereo(clientId, &stereoEnabled) ;
    if ( stereoEnabled == 0 ) {
        SetDoStereo(clientId, 1) ;
		[stereoToolbarItem setImage:[NSImage imageNamed:@"stereo"]] ; 
    } else {
        SetDoStereo(clientId, 0) ;
		[stereoToolbarItem setImage:[NSImage imageNamed:@"stereoTemplate"]] ; 
    }
}

- (double)linearToLog:(double)value
{
    return( pow( pow( 10, value ) / 10.0, 0.3 ) ) ;
}

- (double)logToLinear:(double)value
{
    return( 1.0 + ( log10f( value ) / 0.3 ) ) ;
}

- (IBAction)changeCellVolume:(id)sender
{
	for ( int i=0; i<6; i++ ) {
		mCellVolumes[i] = [[mSliderMatrix cellWithTag:i] floatValue] ;
		mCellVolumes[i] = [self logToLinear:mCellVolumes[i]] ;
		mCellVolumes[i] *= [self logToLinear:mMixVolume] ;
	}
	
    SetGain(clientId, mCellVolumes) ;
	[mResetButton setEnabled:YES] ;
}

- (IBAction)changeCellEnabled:(id)sender 
{
	for ( int i=0; i<6; i++ ) {
		if ( [mButtonMatrix isSelectedForSegment:i] ) {
			mCellEnabled[i] = 1 ;
		} else {
			mCellEnabled[i] = 0 ;
		}
	}
    SetEnabled(clientId, mCellEnabled) ;
	[mResetButton setEnabled:YES] ;
	[sectorView setEnabled:mCellEnabled] ;
	[sectorView setNeedsDisplay:YES] ;
}

- (IBAction)changeMixVolume:(id)sender 
{
	[self changeCellVolume:sender] ;
}

- (IBAction)resetVolumes:(id)sender 
{
	[self setMMixVolume:1.0] ;
	for ( int i=0; i<6; i++ ) {
		mCellVolumes[i] = 1.0 ;
		[[mSliderMatrix cellWithTag:i] setFloatValue:mCellVolumes[i]] ;
		mCellVolumes[i] *= mMixVolume ;
	}
    SetGain(clientId, mCellVolumes) ;
    
	for ( int i=0; i<6; i++ ) {
		mCellEnabled[i] = 1 ;
		[mButtonMatrix setSelectedSegment:i] ;
	}
    SetEnabled(clientId, mCellEnabled) ;
	[sectorView setEnabled:mCellEnabled] ;
	[sectorView setNeedsDisplay:YES] ;
	[mResetButton setEnabled:NO] ;
}

- (void)updateFromDriver:(NSTimer *)aTimer
{
    if ( self.microconeConnected.boolValue == YES ) {
        [self resetInterface:self] ;
    }
}

- (IBAction)onShowConsole:(id)sender
{
    [self.window makeKeyAndOrderFront:self] ;
}


@end
