//
//  MCSectorView.h
//  Microcone Demo
//
//  Copyright 2012 DEV-AUDIO Pty Ltd. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#define NUM_SECTORS 6

@interface MCSectorView : NSView {
	///
	/// \brief Array of booleans indicating whether each sector is enabled or not
	///
	int	mEnabled[NUM_SECTORS] ;
	
	///
	/// \brief Array of floats indicating current position (azimuth, ranging 0->1) within each sector
	///
	float	mAzimuth[NUM_SECTORS] ;
	
	
	///
	/// \brief Array of booleans indicating whether or not there is active speech within each sector
	///
	int		mActive[NUM_SECTORS] ;

	float startDegrees[NUM_SECTORS] ;
	float endDegrees[NUM_SECTORS] ;
	float locationDegrees[NUM_SECTORS] ;
	float cosLocation[NUM_SECTORS] ;
	float sinLocation[NUM_SECTORS] ;
	float ballDegrees[NUM_SECTORS] ;
}

///
/// \brief Settor method for the enabled array
///
- (void)setEnabled:(int *)enabled ;

///
/// \brief Settor method for the azimuth array
///
- (void)setAzimuth:(float *)azimuth ;

///
/// \brief Settor method for the active array
///
- (void)setActive:(int *)active ;

- (NSColor *)colorForTrack:(int)track ;
- (NSColor *) colorFromHexRGB:(NSString *) inColorString ;

@end
