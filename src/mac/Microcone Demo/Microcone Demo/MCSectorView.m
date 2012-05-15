//
//  MCSectorView.m
//  Microcone Demo
//
//  Copyright 2012 DEV-AUDIO Pty Ltd. All rights reserved.
//

#import "MCSectorView.h"

#define NUM_SECTORS		6
#define SECTOR_RANGE	60.0
#define PI				3.14159265

@implementation MCSectorView

- (id)initWithFrame:(NSRect)frame {
    self = [super initWithFrame:frame];
    if (self) {
		// Initialise the member variables to default values
		for ( int i=0; i<NUM_SECTORS; i++ ) {
			mEnabled[i] = 1 ;
			mAzimuth[i] = 0.5 ;
			mActive[i] = 0 ;
			startDegrees[i] = ((i * 60.0) - 29.5) ;
			startDegrees[i] -= 90.0 ;
			endDegrees[i] = ((i * 60.0) + 29.5) ;
			endDegrees[i] -= 90.0 ;
			locationDegrees[i] = startDegrees[i] + (SECTOR_RANGE/2.0) ;
			cosLocation[i] = cos( locationDegrees[i] * PI / 180.0 ) ;
			sinLocation[i] = sin( locationDegrees[i] * PI / 180.0 ) ;
		}
	}
    return self;
}

- (void)drawRect:(NSRect)rect {
	int i ;
	float arcRadius, arcWidth, ballRadius, ballDistance  ;	
	
	// get the bounding rectangle
	NSRect bounds = [self bounds] ;

	// find the centre point
	NSPoint centre ;
	centre.x = bounds.origin.x + ( bounds.size.width / 2 ) ;
	centre.y = bounds.origin.y + ( bounds.size.height / 2 ) ;

	// calculate required dimensions for drawing objects
	float theWidth = bounds.size.width ;
	if ( bounds.size.height < theWidth ) {
		theWidth = bounds.size.height ;
	}
    theWidth *= 0.95 ;
	arcRadius = theWidth / 3 ; 
	arcWidth = theWidth / 3 ;
	ballRadius = theWidth / (6*2.5)  ;
	ballDistance = theWidth / 2.5 ;

	// Initialise the size of rectangle bounding for the speaker activity blobs
	NSRect soundLocation ;
	soundLocation.size.width = 1 + (ballRadius * 2) ;
	soundLocation.size.height = 1 + (ballRadius * 2) ;

	// Initialise the size of rectangle bounding for the sector index labels
	NSRect textLocation ;
	NSPoint centreLocation ;
	
	textLocation.size.width = 10 ;
	textLocation.size.height = 20 ;

	
	[[NSColor colorWithCalibratedRed:0.0 green:0.0 blue:0.0 alpha:1.0] set] ;
	NSRect centreDotBound ;
	centreDotBound.origin.x = centre.x - (arcRadius/2.0) ; 
	centreDotBound.origin.y = centre.y - (arcRadius/2.0) ; 
	centreDotBound.size.width = arcRadius ;
	centreDotBound.size.height = arcRadius ;
	
	NSBezierPath * centreDotPath = [NSBezierPath bezierPathWithOvalInRect:centreDotBound] ;
	[centreDotPath fill] ;
	centreDotPath = nil ;
	

	
	// Now draw each sector
	for ( i=0; i<NUM_SECTORS; i++ ) {
		// set colour depending whether sector is enabled or not
		if ( mEnabled[i] == 1 ) {
			[[NSColor colorWithCalibratedRed:0.64 green:0.82 blue:0.92 alpha:0.8] set] ;
		} else {
			[[NSColor colorWithCalibratedRed:0.25 green:0.25 blue:0.25 alpha:0.35] set] ;
		}

		// now draw the sector region
		NSBezierPath * arcPath = [[NSBezierPath alloc] init] ;
		[arcPath appendBezierPathWithArcWithCenter:centre radius:arcRadius startAngle: startDegrees[i] endAngle:endDegrees[i]] ;
		[arcPath setLineWidth:arcWidth] ;
		[arcPath stroke] ; 
		
		centreLocation.x = centre.x - ( (arcRadius/4.0) * cosLocation[i] ) ;
		centreLocation.y = centre.y - ( (arcRadius/4.0) * sinLocation[i] ) ;

		NSBezierPath * devPath = [[NSBezierPath alloc] init] ;
		[devPath appendBezierPathWithArcWithCenter:centreLocation radius:3.0*arcRadius/4.0 startAngle: startDegrees[i]+8 endAngle:endDegrees[i]-8] ;
		if ( i == 0 ) {
			[[NSColor colorWithCalibratedRed:0 green:0.51 blue:0.78 alpha:1.0] set] ;
		} else {
			[[NSColor colorWithCalibratedRed:0.0 green:0.0 blue:0.0 alpha:1.0] set] ;
		}
		[devPath setLineWidth:arcWidth/10] ;
		[devPath stroke] ; 
		devPath = nil ;
		
		centreLocation.x = centre.x - ( (arcRadius/4.0) * cosLocation[i] ) ;
		centreLocation.y = centre.y - ( (arcRadius/4.0) * sinLocation[i] ) ;
		NSBezierPath * dev2Path = [[NSBezierPath alloc] init] ;
		[dev2Path appendBezierPathWithArcWithCenter:centreLocation radius:2.0*arcRadius/4.0 startAngle: startDegrees[i]+12 endAngle:endDegrees[i]-12] ;
		[[NSColor colorWithCalibratedRed:0.51 green:0.57 blue:0.64 alpha:1.0] set] ;
		[dev2Path setLineWidth:arcWidth/12] ;
		[dev2Path stroke] ; 
		dev2Path = nil ;
				
		// create the string with the sector index
		NSMutableAttributedString * str ;
		str = [[NSMutableAttributedString alloc] initWithString:[[NSString alloc] initWithFormat:@"%d",i+1]] ;
		[str addAttribute:NSFontAttributeName value:[NSFont boldSystemFontOfSize:12] range:NSMakeRange(0,1)] ;
		[str addAttribute:NSForegroundColorAttributeName value:[NSColor whiteColor] range:NSMakeRange(0,1)] ;

		// calculate the location for the index text and the speaker activity blob
		textLocation.origin.x = centre.x + ( (arcRadius/1.05) * cosLocation[i] ) - 2 ;
		textLocation.origin.y = centre.y + ( (arcRadius/1.05) * sinLocation[i] ) - 13 ;

        
        // write the text
		[str drawInRect:textLocation] ;
    }
    
	float curveLength = arcRadius / 5 ;
	float curveWidth = arcRadius / 10 ;
	NSBezierPath * curvePath = [NSBezierPath bezierPath] ;
	NSPoint curveStartPoint ;
	curveStartPoint.x = centre.x ;
	curveStartPoint.y = centre.y + (arcRadius/2.0) ;
	NSPoint curveExtent ;
	curveExtent.x = curveWidth ;
	curveExtent.y = curveLength ;
	NSPoint leftCtrlPoint, rightCtrlPoint ;
	leftCtrlPoint.x = -1.0 * curveWidth ;
	rightCtrlPoint.x = 2 * curveWidth ;
	leftCtrlPoint.y = curveLength/3 ;
	rightCtrlPoint.y = 2 * curveLength / 3 ;
	[curvePath moveToPoint:curveStartPoint] ;
	[curvePath relativeCurveToPoint:curveExtent controlPoint1:leftCtrlPoint controlPoint2:rightCtrlPoint] ;
	[curvePath setLineWidth:3] ;
	[[NSColor colorWithCalibratedRed:0.0 green:0.0 blue:0.0 alpha:1.0] set] ;
	[curvePath stroke] ; 
	curvePath = nil ;		
	
    for ( i=0; i<NUM_SECTORS; i++ ) {
		ballDegrees[i] = locationDegrees[i] + (( mAzimuth[i] - 0.5 ) * 60.0 ) ;
		soundLocation.origin.x = centre.x + ( ballDistance * cos( ballDegrees[i] * PI / 180.0 ) ) - (soundLocation.size.width/2) ;
		soundLocation.origin.y = centre.y + ( ballDistance * sin( ballDegrees[i] * PI / 180.0 ) ) - (soundLocation.size.height/2) ;

		// if the sector is enabled and there is speech activity, then draw the blob
		if ( (mEnabled[i] == 1) && (mActive[i] == 1) ) {
            [[self colorForTrack:i] set] ; 
			NSBezierPath * ovalPath = [NSBezierPath bezierPathWithOvalInRect:soundLocation] ;
			[ovalPath fill] ;
		}
	}
}

- (void)setEnabled:(int *)enabled {
	// use memcpy as should be faster than a for loop
	memcpy( mEnabled, enabled, NUM_SECTORS * sizeof(int) ) ;
}

- (void)setAzimuth:(float *)azimuth {
	// use memcpy as should be faster than a for loop
	memcpy( mAzimuth, azimuth, NUM_SECTORS * sizeof(float) ) ;	
}

- (void)setActive:(int *)active {
	// use memcpy as should be faster than a for loop
	memcpy( mActive, active, NUM_SECTORS * sizeof(int) ) ;	
}

- (NSColor *) colorFromHexRGB:(NSString *) inColorString
{
	NSColor *result = nil;
	unsigned int colorCode = 0;
	unsigned char redByte, greenByte, blueByte;
	
	if (nil != inColorString)
	{
		NSScanner *scanner = [NSScanner scannerWithString:inColorString];
		(void) [scanner scanHexInt:&colorCode];	// ignore error
	}
	redByte		= (unsigned char) (colorCode >> 16);
	greenByte	= (unsigned char) (colorCode >> 8);
	blueByte	= (unsigned char) (colorCode);	// masks off high bits
	result = [NSColor
              colorWithCalibratedRed:		(float)redByte	/ 0xff
              green:	(float)greenByte/ 0xff
              blue:	(float)blueByte	/ 0xff
              alpha:1.0];
	return result;
}

- (NSColor *)colorForTrack:(int)track
{
    NSString * hexColors[10] = {@"0652A3",@"0981FF",@"127100",@"1FC752",@"642887",@"A36AB9",@"887766",@"bbaabb",@"bb3344",@"88dddd"} ;
    return ( [self colorFromHexRGB:hexColors[track]] ) ;
}


@end
