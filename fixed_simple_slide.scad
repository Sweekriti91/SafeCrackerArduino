// Simple Cam-Lock Sliding Lid Box for MG996R Servo
// SafeCrackerArduino Reaction Time Game
// No gravity assist - pure cam mechanism

// MG996R dimensions
servo_length = 40;
servo_width = 20;
servo_height = 40.5;
servo_mount_width = 55;

// Box parameters
wall = 2.5;
clearance = 0.5;
base_thickness = 3;
size_increase = 1.1; // 10% increase factor

// Sliding mechanism
slide_groove_depth = 2;
slide_groove_width = 3;
lid_thickness = 3;

// Cam parameters - REDUCED SIZE
cam_diameter = 20;        // Reduced from 30 to 20mm
cam_thickness = 5;
cam_flat_width = 12;      // Reduced flat width proportionally

// Calculate box dimensions with 10% increase
box_inner_l = (servo_length + 2 * clearance) * size_increase;
box_inner_w = (servo_mount_width + 2 * clearance) * size_increase;
box_inner_h = (servo_height + clearance) * size_increase;
box_outer_l = box_inner_l + 2 * wall;
box_outer_w = box_inner_w + 2 * wall;
box_outer_h = box_inner_h + base_thickness;

// Main box - NO SERVO MOUNTS
module box() {
    difference() {
        // Outer shell
        cube([box_outer_l, box_outer_w, box_outer_h]);
        
        // Inner cavity - clean empty space for servo
        translate([wall, wall, base_thickness])
            cube([box_inner_l, box_inner_w, box_inner_h + 10]);
        
        // Servo shaft hole - large for easy assembly
        translate([box_outer_l/2, box_outer_w/2, -1])
            cylinder(h=base_thickness + 2, d=12, $fn=24);
        
        // Cable exit
        translate([box_outer_l - wall - 1, box_outer_w/2 - 5, base_thickness + 15])
            cube([wall + 2, 10, 10]);
        
        // Slide grooves for lid
        groove_height = box_outer_h - 8;
        // Left groove
        translate([-1, wall - slide_groove_width/2, groove_height])
            cube([box_outer_l + 2, slide_groove_width, slide_groove_depth + 1]);
        // Right groove
        translate([-1, box_outer_w - wall - slide_groove_width/2, groove_height])
            cube([box_outer_l + 2, slide_groove_width, slide_groove_depth + 1]);
        
        // Front opening for lid
        translate([-1, wall - slide_groove_width/2, groove_height])
            cube([wall + 2, box_inner_w + slide_groove_width + 1, 10]);
        
        // Mounting holes
        for(x=[8, box_outer_l - 8]) {
            for(y=[8, box_outer_w - 8]) {
                translate([x, y, -1]) {
                    cylinder(h=base_thickness + 2, d=3.2, $fn=16);
                    // Countersink
                    cylinder(h=2, d1=6, d2=3.2, $fn=16);
                }
            }
        }
    }
    
    // NO SERVO MOUNTS - servo sits freely in cavity
}

// Simple sliding lid
module sliding_lid() {
    lid_length = box_outer_l - wall - 0.5;
    lid_width = box_inner_w - 0.5;
    
    difference() {
        union() {
            // Main lid panel
            cube([lid_length, lid_width, lid_thickness]);
            
            // Slide rails
            rail_thickness = slide_groove_width - 0.3;
            rail_height = slide_groove_depth - 0.2;
            
            // Left rail
            translate([0, -rail_thickness/2 + 0.1, lid_thickness])
                cube([lid_length - 5, rail_thickness, rail_height]);
            // Right rail
            translate([0, lid_width - rail_thickness/2 - 0.1, lid_thickness])
                cube([lid_length - 5, rail_thickness, rail_height]);
            
            // Pull handle
            translate([lid_length - 15, lid_width/2 - 10, 0])
                hull() {
                    cube([10, 20, lid_thickness]);
                    translate([5, 10, lid_thickness])
                        cylinder(h=2, d=15, $fn=24);
                }
            
            // Cam follower block at front - REDUCED HEIGHT
            translate([0, lid_width/2 - 6, 0])  // Reduced from 8 to 6
                cube([5, 12, lid_thickness + 6]);  // Reduced width and height
        }
        
        // Cam contact surface (rounded for smooth operation) - ADJUSTED
        translate([2.5, lid_width/2, lid_thickness + 3])  // Lowered contact point
            rotate([0, 90, 0])
                cylinder(h=6, d=6, $fn=16, center=true);  // Smaller contact surface
        
        // Status window
        translate([lid_length/2 - 15, lid_width/2 - 4, -1])
            cube([30, 8, lid_thickness + 2]);
        
        // Text
        translate([lid_length/2, lid_width/2, lid_thickness - 0.5])
            linear_extrude(height=1)
                text("LOCKED", size=5, halign="center", valign="center");
    }
}

// Smaller D-shaped cam
module simple_cam() {
    difference() {
        // Main cam shape - SMALLER
        intersection() {
            cylinder(h=cam_thickness, d=cam_diameter, $fn=32);
            translate([-cam_diameter/2, -cam_diameter/2, 0])
                cube([cam_diameter, cam_diameter - cam_flat_width/2 + cam_diameter/2, cam_thickness]);
        }
        
        // Center hole for servo shaft
        translate([0, 0, -1])
            cylinder(h=cam_thickness + 2, d=6.2, $fn=24);
        
        // Set screw hole - ADJUSTED for smaller cam
        translate([0, 6, cam_thickness/2])  // Moved closer to center
            rotate([0, 90, 0])
                cylinder(h=15, d=3, $fn=16, center=true);
        
        // Position marker line for assembly reference
        translate([-1, 0, cam_thickness - 0.5])
            cube([cam_diameter/2 + 1, 1.5, 0.5]);  // Smaller marker
    }
}

// Assembly view
// Box (no servo mounts)
//box();

// Lid (shown separately for printing)
//translate([0, box_outer_w + 10, 0])
  //  sliding_lid();

// Cam (shown separately) - smaller size
//translate([box_outer_l + 20, 20, 0])
    simple_cam();

// Assembly instructions
echo("=== ASSEMBLY INSTRUCTIONS ===");
echo("1. Drop servo into box cavity (no mounts blocking)");
echo("2. Slide lid in from front (cam follower faces inward)");
echo("3. Attach smaller cam to servo shaft with flat edge at 0°");
echo("4. Cam at 0-30°: Flat blocks lid (LOCKED)");
echo("5. Cam at 90-180°: Round edge allows sliding (UNLOCKED)");
echo("Print time: ~2.5 hours total at 0.2mm layers");
echo("Servo mounts removed - clean cavity for easy insertion");
echo("Cam size reduced from 30mm to 20mm diameter");