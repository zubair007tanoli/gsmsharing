import React, { useState } from "react";
import {
  View,
  Text,
  StyleSheet,
  Pressable,
  useColorScheme,
  Platform,
  ScrollView,
  Switch,
  Alert,
} from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Ionicons } from "@expo/vector-icons";
import { router } from "expo-router";
import * as Haptics from "expo-haptics";
import Colors from "@/constants/colors";
import { useDocuments } from "@/context/DocumentsContext";

function SettingRow({
  icon,
  iconBg,
  label,
  value,
  onPress,
  showArrow = true,
  destructive = false,
  rightElement,
}: {
  icon: string;
  iconBg: string;
  label: string;
  value?: string;
  onPress?: () => void;
  showArrow?: boolean;
  destructive?: boolean;
  rightElement?: React.ReactNode;
}) {
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;

  return (
    <Pressable
      style={({ pressed }) => [
        styles.settingRow,
        { backgroundColor: theme.card, opacity: pressed ? 0.75 : 1 },
      ]}
      onPress={() => {
        Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light);
        onPress?.();
      }}
    >
      <View style={[styles.settingIcon, { backgroundColor: iconBg }]}>
        <Ionicons name={icon as any} size={18} color="#fff" />
      </View>
      <Text style={[
        styles.settingLabel,
        { color: destructive ? "#EF4444" : theme.text, fontFamily: "Inter_500Medium" },
      ]}>
        {label}
      </Text>
      <View style={styles.settingRight}>
        {value && (
          <Text style={[styles.settingValue, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
            {value}
          </Text>
        )}
        {rightElement}
        {showArrow && !rightElement && (
          <Ionicons name="chevron-forward" size={16} color={theme.textMuted} />
        )}
      </View>
    </Pressable>
  );
}

function SectionTitle({ title }: { title: string }) {
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;
  return (
    <Text style={[styles.sectionTitle, { color: theme.textMuted, fontFamily: "Inter_600SemiBold" }]}>
      {title}
    </Text>
  );
}

function SectionGroup({ children }: { children: React.ReactNode }) {
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;
  return (
    <View style={[styles.group, { borderColor: theme.border }]}>
      {children}
    </View>
  );
}

export default function SettingsScreen() {
  const colorScheme = useColorScheme();
  const isDark = colorScheme === "dark";
  const theme = isDark ? Colors.dark : Colors.light;
  const insets = useSafeAreaInsets();
  const { documents, totalSize } = useDocuments();
  const [notifications, setNotifications] = useState(true);
  const [autoBackup, setAutoBackup] = useState(false);
  const [highQuality, setHighQuality] = useState(true);
  const [autoOcr, setAutoOcr] = useState(false);

  const topPad = Platform.OS === "web" ? Math.max(insets.top, 67) : insets.top;

  return (
    <View style={[styles.container, { backgroundColor: theme.background }]}>
      {/* Header */}
      <View style={[styles.header, { paddingTop: topPad + 10, backgroundColor: theme.surface, borderBottomColor: theme.border }]}>
        <Pressable onPress={() => router.back()} style={styles.backBtn}>
          <Ionicons name="chevron-back" size={24} color={theme.primary} />
        </Pressable>
        <Text style={[styles.headerTitle, { color: theme.text, fontFamily: "Inter_700Bold" }]}>
          Settings
        </Text>
        <View style={{ width: 40 }} />
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={{ paddingBottom: Platform.OS === "web" ? 34 : insets.bottom + 30, paddingTop: 24 }}
      >
        {/* Profile Card */}
        <View style={[styles.profileCard, { backgroundColor: theme.primary }]}>
          <View style={styles.profileAvatar}>
            <Text style={[styles.profileAvatarText, { fontFamily: "Inter_700Bold" }]}>P</Text>
          </View>
          <View style={styles.profileInfo}>
            <Text style={[styles.profileName, { fontFamily: "Inter_700Bold" }]}>PdfPeaks User</Text>
            <Text style={[styles.profilePlan, { fontFamily: "Inter_400Regular" }]}>Free Plan</Text>
          </View>
          <Pressable
            style={styles.upgradeBtn}
            onPress={() => Alert.alert("Upgrade to Pro", "Get unlimited scans, cloud backup, OCR and more.", [{ text: "Later" }, { text: "Upgrade", onPress: () => {} }])}
          >
            <Text style={[styles.upgradeBtnText, { fontFamily: "Inter_600SemiBold" }]}>Upgrade</Text>
          </Pressable>
        </View>

        {/* Storage Stats */}
        <View style={[styles.storageCard, { backgroundColor: theme.card }]}>
          <View style={styles.storageRow}>
            <Ionicons name="server-outline" size={20} color={theme.primary} />
            <Text style={[styles.storageLabel, { color: theme.text, fontFamily: "Inter_500Medium" }]}>
              Storage Used
            </Text>
            <Text style={[styles.storageValue, { color: theme.primary, fontFamily: "Inter_700Bold" }]}>
              {totalSize}
            </Text>
          </View>
          <View style={[styles.storageBarBg, { backgroundColor: theme.surfaceSecondary }]}>
            <View style={[styles.storageBarFill, { backgroundColor: theme.primary, width: "18%" }]} />
          </View>
          <Text style={[styles.storageNote, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
            {documents.length} files · 500 MB free limit
          </Text>
        </View>

        {/* Scan Settings */}
        <View style={styles.section}>
          <SectionTitle title="SCAN" />
          <SectionGroup>
            <SettingRow
              icon="camera"
              iconBg="#2563EB"
              label="Scan Quality"
              value="High"
              onPress={() => Alert.alert("Scan Quality", "Choose scan quality", [
                { text: "Standard" }, { text: "High" }, { text: "Ultra" },
              ])}
            />
            <View style={[styles.rowSep, { backgroundColor: theme.border }]} />
            <SettingRow
              icon="text"
              iconBg="#06B6D4"
              label="Auto OCR on Scan"
              showArrow={false}
              rightElement={
                <Switch
                  value={autoOcr}
                  onValueChange={setAutoOcr}
                  trackColor={{ false: theme.border, true: theme.primary }}
                  thumbColor="#fff"
                />
              }
            />
            <View style={[styles.rowSep, { backgroundColor: theme.border }]} />
            <SettingRow
              icon="image"
              iconBg="#8B5CF6"
              label="High Quality Scans"
              showArrow={false}
              rightElement={
                <Switch
                  value={highQuality}
                  onValueChange={setHighQuality}
                  trackColor={{ false: theme.border, true: theme.primary }}
                  thumbColor="#fff"
                />
              }
            />
          </SectionGroup>
        </View>

        {/* Storage & Backup */}
        <View style={styles.section}>
          <SectionTitle title="STORAGE & BACKUP" />
          <SectionGroup>
            <SettingRow
              icon="cloud-upload"
              iconBg="#10B981"
              label="Cloud Backup"
              showArrow={false}
              rightElement={
                <Switch
                  value={autoBackup}
                  onValueChange={setAutoBackup}
                  trackColor={{ false: theme.border, true: theme.primary }}
                  thumbColor="#fff"
                />
              }
            />
            <View style={[styles.rowSep, { backgroundColor: theme.border }]} />
            <SettingRow
              icon="folder-open"
              iconBg="#F59E0B"
              label="Default Save Location"
              value="My Documents"
              onPress={() => {}}
            />
            <View style={[styles.rowSep, { backgroundColor: theme.border }]} />
            <SettingRow
              icon="trash"
              iconBg="#EF4444"
              label="Clear Cache"
              value="12.4 MB"
              onPress={() => Alert.alert("Clear Cache", "This will free up space.", [
                { text: "Cancel" }, { text: "Clear", style: "destructive", onPress: () => {} },
              ])}
            />
          </SectionGroup>
        </View>

        {/* Notifications */}
        <View style={styles.section}>
          <SectionTitle title="NOTIFICATIONS" />
          <SectionGroup>
            <SettingRow
              icon="notifications"
              iconBg="#F59E0B"
              label="Push Notifications"
              showArrow={false}
              rightElement={
                <Switch
                  value={notifications}
                  onValueChange={setNotifications}
                  trackColor={{ false: theme.border, true: theme.primary }}
                  thumbColor="#fff"
                />
              }
            />
          </SectionGroup>
        </View>

        {/* Account */}
        <View style={styles.section}>
          <SectionTitle title="ACCOUNT" />
          <SectionGroup>
            <SettingRow
              icon="star"
              iconBg="#F59E0B"
              label="Upgrade to Pro"
              value="$4.99/mo"
              onPress={() => Alert.alert("Pro Plan", "Unlock all features with Pro.")}
            />
            <View style={[styles.rowSep, { backgroundColor: theme.border }]} />
            <SettingRow
              icon="shield-checkmark"
              iconBg="#10B981"
              label="Privacy Policy"
              onPress={() => {}}
            />
            <View style={[styles.rowSep, { backgroundColor: theme.border }]} />
            <SettingRow
              icon="document-text"
              iconBg="#64748B"
              label="Terms of Service"
              onPress={() => {}}
            />
            <View style={[styles.rowSep, { backgroundColor: theme.border }]} />
            <SettingRow
              icon="information-circle"
              iconBg="#2563EB"
              label="App Version"
              value="1.0.0"
              showArrow={false}
            />
          </SectionGroup>
        </View>

        {/* Danger Zone */}
        <View style={styles.section}>
          <SectionTitle title="DANGER ZONE" />
          <SectionGroup>
            <SettingRow
              icon="trash"
              iconBg="#EF4444"
              label="Delete All Documents"
              destructive
              onPress={() => Alert.alert(
                "Delete All Documents",
                "This will permanently delete all your documents. This cannot be undone.",
                [
                  { text: "Cancel" },
                  { text: "Delete All", style: "destructive", onPress: () => {} },
                ]
              )}
            />
          </SectionGroup>
        </View>
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  header: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 12,
    paddingBottom: 12,
    borderBottomWidth: StyleSheet.hairlineWidth,
  },
  backBtn: { padding: 4 },
  headerTitle: { flex: 1, fontSize: 20, textAlign: "center" },
  profileCard: {
    marginHorizontal: 16,
    borderRadius: 20,
    padding: 20,
    flexDirection: "row",
    alignItems: "center",
    gap: 14,
    marginBottom: 16,
  },
  profileAvatar: {
    width: 52,
    height: 52,
    borderRadius: 26,
    backgroundColor: "rgba(255,255,255,0.25)",
    alignItems: "center",
    justifyContent: "center",
  },
  profileAvatarText: { color: "#fff", fontSize: 22 },
  profileInfo: { flex: 1 },
  profileName: { color: "#fff", fontSize: 17, marginBottom: 2 },
  profilePlan: { color: "rgba(255,255,255,0.8)", fontSize: 13 },
  upgradeBtn: { backgroundColor: "rgba(255,255,255,0.25)", paddingHorizontal: 14, paddingVertical: 8, borderRadius: 10 },
  upgradeBtnText: { color: "#fff", fontSize: 13 },
  storageCard: { marginHorizontal: 16, borderRadius: 16, padding: 16, marginBottom: 8, gap: 10 },
  storageRow: { flexDirection: "row", alignItems: "center", gap: 10 },
  storageLabel: { flex: 1, fontSize: 15 },
  storageValue: { fontSize: 15 },
  storageBarBg: { height: 6, borderRadius: 3, overflow: "hidden" },
  storageBarFill: { height: 6, borderRadius: 3 },
  storageNote: { fontSize: 12 },
  section: { marginTop: 24, paddingHorizontal: 16 },
  sectionTitle: { fontSize: 12, marginBottom: 10, paddingHorizontal: 4 },
  group: { borderRadius: 16, overflow: "hidden", borderWidth: StyleSheet.hairlineWidth },
  settingRow: { flexDirection: "row", alignItems: "center", padding: 14, gap: 12 },
  settingIcon: { width: 32, height: 32, borderRadius: 9, alignItems: "center", justifyContent: "center" },
  settingLabel: { flex: 1, fontSize: 15 },
  settingRight: { flexDirection: "row", alignItems: "center", gap: 6 },
  settingValue: { fontSize: 14 },
  rowSep: { height: StyleSheet.hairlineWidth, marginLeft: 58 },
});
