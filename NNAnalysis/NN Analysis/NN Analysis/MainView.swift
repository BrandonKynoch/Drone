//
//  ContentView.swift
//  NN Analysis
//
//  Created by Brandon Kynoch on 2022/07/10.
//

import SwiftUI

let S_COL_MAIN: Color = Color(nsColor: NSColor(hex: "#F20519")!)
let S_COL_ACC1: Color = Color(nsColor: NSColor(hex: "#F22738")!)
let S_COL_ACC2: Color = Color(nsColor: NSColor(hex: "#F27E7E")!)
let S_COL_BACKGROUND: Color = Color(nsColor: NSColor(hex: "#262223")!)
let S_COL_BACKGROUND2: Color = Color(nsColor: NSColor(hex: "#F2F2F2")!)

let S_COL_CANCEL: Color = Color(nsColor: NSColor(hex: "#DE1D12")!)
let S_COL_APPROVE: Color = Color(nsColor: NSColor(hex: "#49DE1D")!)

let S_SHADOW_COL: Color = Color.black.opacity(0.1)

let S_CORNER_RADIUS: CGFloat = 12

enum ViewMode {
    case light
    case dark
}

struct MainView: View {
    var body: some View {
        NavigationView {
            SideBarView()
            .toolbar {
                ToolbarItem(placement: .navigation) {
                    Button(action: toggleSidebar, label: { // 1
                        Image(systemName: "sidebar.leading")
                    })
                }
            }
            
            NNGraphView()
        }
    }
    
    private func toggleSidebar() { // 2
        #if os(iOS)
        #else
        NSApp.keyWindow?.firstResponder?.tryToPerform(#selector(NSSplitViewController.toggleSidebar(_:)), with: nil)
        #endif
    }
}

struct MainView_Previews: PreviewProvider {
    static var previews: some View {
        MainView()
    }
}
