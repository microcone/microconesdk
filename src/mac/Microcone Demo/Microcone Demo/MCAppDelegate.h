//
//  MCAppDelegate.h
//  Microcone Demo
//
//  Copyright 2012 DEV-AUDIO Pty Ltd. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#import "MCSectorView.h"

@interface MCAppDelegate : NSObject <NSApplicationDelegate> {
    SInt32 clientId ;
    MCSectorView *                  sectorView ;
    NSNumber *                      microconeConnected ;
    
	float							mCellVolumes[6] ;
	int								mCellEnabled[6] ;
    float                           mMixVolume ;
    
	IBOutlet NSToolbarItem *        dspToolbarItem ;
	IBOutlet NSToolbarItem *        stereoToolbarItem ;
    
    IBOutlet NSSlider *				mMixSlider ;
    IBOutlet NSMatrix *				mSliderMatrix ;
    IBOutlet NSSegmentedControl *	mButtonMatrix ;
    IBOutlet NSButton *				mResetButton ;
    
    NSTimer *                       mUpdateTimer ;
}

@property (assign) IBOutlet NSWindow *window;
@property (retain,readwrite) IBOutlet MCSectorView * sectorView;
@property (readwrite, assign) float mMixVolume ;
@property (readwrite, retain) NSNumber * microconeConnected ;

- (double)linearToLog:(double)value ;
- (double)logToLinear:(double)value ;


- (IBAction)resetVolumes:(id)sender ;
- (IBAction)changeCellVolume:(id)sender ;
- (IBAction)changeCellEnabled:(id)sender ;
- (IBAction)changeMixVolume:(id)sender ;

- (IBAction)onEnableDSP:(id)sender;
- (IBAction)onEnableStereo:(id)sender;

- (IBAction)onShowConsole:(id)sender ;

- (void)connected:(NSNotification *)note ;
- (void)resetInterface:(id)sender ;
- (void)disconnected:(NSNotification *)note ;

- (void)updateFromDriver:(NSTimer *)aTimer ;

@end
